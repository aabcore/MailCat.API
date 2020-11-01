using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using MailCat.API.Models.ApiDtos;
using MailCat.API.Models.Entities;
using MailCat.API.Models.Filters;
using MailCat.API.Results;
using MailCat.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace MailCat.API.Controllers
{
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private MailService MailService { get; }
        private IMapper Mapper { get; }

        public MailController(MailService mailService, IMapper mapper)
        {
            MailService = mailService;
            Mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(FilteredDtoOut<GetMailFilter, MailOutDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Always returns in desc datetime order
        public async Task<IActionResult> GetMail(DateTimeOffset? before = null, DateTimeOffset? after = null, string toEmail = null,
            string fromEmail = null, int limit = 100,
            int skip = 0)
        {
            var getMailFilter = new GetMailFilter
            {
                Before = before,
                After = after,
                ToEmail = toEmail,
                FromEmail = fromEmail,
                Limit = Math.Min(limit, 1000),
                Skip = Math.Max(0, skip)
            };

            var getMailResult = await MailService.GetMail(getMailFilter);
            switch (getMailResult)
            {
                case BadRequestTypedResult<IEnumerable<MailEntity>> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<IEnumerable<MailEntity>> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<IEnumerable<MailEntity>> _:
                    return NotFound();
                case SuccessfulTypedResult<IEnumerable<MailEntity>> successfulTypedResult:
                    return Ok(new FilteredDtoOut<GetMailFilter, MailOutDto>(getMailFilter, Mapper.Map<IEnumerable<MailEntity>, IEnumerable<MailOutDto>>(successfulTypedResult.Value)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(getMailResult));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendEmail(NewEmailInDto newMailIn)
        {
            var newMailResult = await MailService.CreateMail(newMailIn);
            switch (newMailResult)
            {
                case BadRequestTypedResult<bool> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<bool> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case SuccessfulTypedResult<bool> _:
                    return NoContent();
                default:
                    throw new ArgumentOutOfRangeException(nameof(newMailResult));
            }
        }
    }

    public class NewEmailInDto
    {
        [Required]
        [EmailAddressRex]
        public string From { get; set; }

        [Required]
        [MinLength(1)]
        [EmailAddressRex]
        public IEnumerable<string> ToRecipients { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> CcRecipients { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> BccRecipients { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailAddressRexAttribute : ValidationAttribute
    {
        private static Regex EmailRegex = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            switch (value)
            {
                case null:
                    return ValidationResult.Success;
                case string strVal:
                    return EmailRegex.IsMatch(strVal)
                        ? ValidationResult.Success
                        : new ValidationResult($"{strVal} does not look like an email address.");
                case IEnumerable<string> arrVal:
                {
                    var collectedErrors = arrVal.Where(strVal => !EmailRegex.IsMatch(strVal)).Select(strVal => $"{strVal}").ToList();

                    return collectedErrors.Any()
                        ? new ValidationResult($"[{string.Join($", ", collectedErrors)}] don't look like email address(es)")
                        : ValidationResult.Success;
                }
                default:
                    return new ValidationResult("Input doesn't match a string or a string[].");
            }
        }
    }
}