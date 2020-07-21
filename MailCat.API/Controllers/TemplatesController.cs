using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using HandlebarsDotNet;
using MailCat.API.Models.ApiDtos;
using MailCat.API.Models.Entities;
using MailCat.API.Models.Filters;
using MailCat.API.Results;
using MailCat.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace MailCat.API.Controllers
{
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private MailService _mailService { get; }
        private IMapper Mapper { get; }
        private TemplateService _templateService;

        public TemplatesController(TemplateService templateService, MailService mailService, IMapper mapper)
        {
            _mailService = mailService;
            Mapper = mapper;
            _templateService = templateService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(TemplateOutDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateNewTemplate(TemplateInDto templateIn)
        {
            var createTemplateResult = await _templateService.NewTemplate(templateIn);
            switch (createTemplateResult)
            {
                case BadRequestTypedResult<TemplateEntity> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<TemplateEntity> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<TemplateEntity> notFoundTypedResult:
                    return NotFound();
                case SuccessfulTypedResult<TemplateEntity> successfulTypedResult:
                    return Ok(Mapper.Map<TemplateEntity, TemplateOutDto>(successfulTypedResult.Value));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(createTemplateResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(FilteredDtoOut<GetTemplatesFilter, TemplateOutDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTemplates(DateTimeOffset? dateBefore = null, string nameContains = null,
            string descriptionContains = null, int limit = 100, int skip = 0)
        {
            var getTemplatesFilter = new GetTemplatesFilter()
            {
                NameContains = nameContains,
                DescriptionContains = descriptionContains,
                DateBefore = dateBefore,
                Limit = Math.Clamp(limit, 0, 1000),
                Skip = Math.Max(0, skip)
            };

            var getTemplatesResult = await _templateService.GetTemplates(getTemplatesFilter);
            switch (getTemplatesResult)
            {
                case BadRequestTypedResult<IEnumerable<TemplateEntity>> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<IEnumerable<TemplateEntity>> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<IEnumerable<TemplateEntity>> _:
                    return NotFound();
                case SuccessfulTypedResult<IEnumerable<TemplateEntity>> successfulTypedResult:
                    return Ok(new FilteredDtoOut<GetTemplatesFilter, TemplateOutDto>(getTemplatesFilter,
                        Mapper.Map<IEnumerable<TemplateEntity>, IEnumerable<TemplateOutDto>>(successfulTypedResult.Value)));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(getTemplatesResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpGet]
        [Route("{templateId}")]
        [ProducesResponseType(typeof(TemplateOutDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSingleTemplate([FromRoute, ObjectIdValidation] string templateId)
        {
            var getTemplateResult = await _templateService.GetTemplate(ObjectId.Parse(templateId));

            switch (getTemplateResult)
            {
                case BadRequestTypedResult<TemplateEntity> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<TemplateEntity> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<TemplateEntity> _:
                    return NotFound();
                case SuccessfulTypedResult<TemplateEntity> successfulTypedResult:
                    return Ok(Mapper.Map<TemplateEntity, TemplateOutDto>(successfulTypedResult.Value));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(getTemplateResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpPut]
        [Route("{templateId}")]
        [ProducesResponseType(typeof(TemplateOutDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTemplate([FromRoute, ObjectIdValidation] string templateId, [FromBody] TemplateInDto templateIn)
        {
            var updateTemplateResult = await _templateService.UpdateTemplate(ObjectId.Parse(templateId), templateIn);

            switch (updateTemplateResult)
            {
                case BadRequestTypedResult<TemplateEntity> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<TemplateEntity> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<TemplateEntity> _:
                    return NotFound();
                case SuccessfulTypedResult<TemplateEntity> successfulTypedResult:
                    return Ok(Mapper.Map<TemplateEntity, TemplateOutDto>(successfulTypedResult.Value));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(updateTemplateResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpPost]
        [Route("{templateId}/TemplateRevisions")]
        [ProducesResponseType(typeof(TemplateRevisionOutDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> NewTemplateRevision([FromRoute, ObjectIdValidation] string templateId,
            [FromBody] TemplateRevisionInDto templateRevisionIn)
        {
            var createTemplateResult = await _templateService.NewTemplateRevision(ObjectId.Parse(templateId), templateRevisionIn);
            switch (createTemplateResult)
            {
                case BadRequestTypedResult<TemplateRevisionEntity> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<TemplateRevisionEntity> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<TemplateRevisionEntity> _:
                    return NotFound();
                case SuccessfulTypedResult<TemplateRevisionEntity> successfulTypedResult:
                    return Ok(Mapper.Map<TemplateRevisionEntity, TemplateRevisionOutDto>(successfulTypedResult.Value));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(createTemplateResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpGet]
        [Route("{templateId}/TemplateRevisions")]
        [ProducesResponseType(typeof(TemplateRevisionOutDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTemplateRevisions([FromRoute, ObjectIdValidation] string templateId, int limit = 100, int skip = 0,
            DateTimeOffset? dateBefore = null)
        {
            var getTemplateRevisionsFilter = new GetTemplateRevisionsFilter()
            {
                Limit = Math.Clamp(limit, 0, 1000),
                Skip = Math.Max(0, skip),
                DateBefore = dateBefore
            };
            var getTemplateRevisionResult =
                await _templateService.GetFilteredTemplateRevisions(ObjectId.Parse(templateId), getTemplateRevisionsFilter);

            switch (getTemplateRevisionResult)
            {
                case BadRequestTypedResult<IEnumerable<TemplateRevisionEntity>> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<IEnumerable<TemplateRevisionEntity>> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<IEnumerable<TemplateRevisionEntity>> _:
                    return NotFound();
                case SuccessfulTypedResult<IEnumerable<TemplateRevisionEntity>> successfulTypedResult:
                    return Ok(new FilteredDtoOut<GetTemplateRevisionsFilter, TemplateRevisionOutDto>(getTemplateRevisionsFilter,
                        Mapper.Map<IEnumerable<TemplateRevisionEntity>, IEnumerable<TemplateRevisionOutDto>>(successfulTypedResult.Value)));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(getTemplateRevisionResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpGet]
        [Route("{templateId}/TemplateRevisions/{templateRevisionId}")]
        [ProducesResponseType(typeof(TemplateRevisionOutDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSingleTemplateRevision([FromRoute, ObjectIdValidation] string templateId,
            [FromRoute, ObjectIdValidation] string templateRevisionId)
        {
            var getTemplateRevisionResult =
                await _templateService.GetTemplateRevision(ObjectId.Parse(templateId), ObjectId.Parse(templateRevisionId));

            switch (getTemplateRevisionResult)
            {
                case BadRequestTypedResult<TemplateRevisionEntity> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<TemplateRevisionEntity> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<TemplateRevisionEntity> _:
                    return NotFound();
                case SuccessfulTypedResult<TemplateRevisionEntity> successfulTypedResult:
                    return Ok(Mapper.Map<TemplateRevisionEntity, TemplateRevisionOutDto>(successfulTypedResult.Value));
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(getTemplateRevisionResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Route("{templateId}/send")]
        public async Task<IActionResult> Send([FromRoute, ObjectIdValidation] string templateId, SendInDto sendIn)
        {
            var foundTemplateResult = await _templateService.GetLatestTemplateRevision(ObjectId.Parse(templateId));

            TemplateRevisionEntity foundTemplateEntity;
            switch (foundTemplateResult)
            {
                case BadRequestTypedResult<TemplateRevisionEntity> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<TemplateRevisionEntity> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<TemplateRevisionEntity> _:
                    return NotFound();
                case SuccessfulTypedResult<TemplateRevisionEntity> successfulTypedResult:
                    foundTemplateEntity = successfulTypedResult.Value;
                    break;
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(foundTemplateResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }

            if (string.IsNullOrWhiteSpace(sendIn.From) && string.IsNullOrWhiteSpace(foundTemplateEntity.DefaultFrom))
            {
                ModelState.AddModelError(nameof(sendIn.From), $"{nameof(sendIn.From)} is required");
                return ValidationProblem(ModelState);
            }

            var newMailIn = new NewEmailInDto()
            {
                From = sendIn.From ?? foundTemplateEntity.DefaultFrom,
                ToRecipients = sendIn.ToRecipients ?? foundTemplateEntity.DefaultToRecipients,
                CcRecipients = sendIn.CcRecipients ?? foundTemplateEntity.DefaultCcRecipients,
                BccRecipients = sendIn.BccRecipients ?? foundTemplateEntity.DefaultBccRecipients,
                Subject = Handlebars.Compile(foundTemplateEntity.SubjectTemplate)(sendIn.Data),
                Body = Handlebars.Compile(foundTemplateEntity.BodyTemplate)(sendIn.Data)
            };

            var newMailResult = await _mailService.CreateMail(newMailIn);
            switch(newMailResult)
            {
                case BadRequestTypedResult<bool> badRequestTypedResult:
                    ModelState.AddModelError(badRequestTypedResult.Key, badRequestTypedResult.ErrorMessage);
                    return ValidationProblem(ModelState);
                case FailedTypedResult<bool> failedTypedResult:
                    return Problem(failedTypedResult.Error.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: failedTypedResult.Error.Message);
                case NotFoundTypedResult<bool> _:
                    return NotFound();
                case SuccessfulTypedResult<bool> _:
                    return NoContent();
                default:
                {
                    var argException = new ArgumentOutOfRangeException(nameof(foundTemplateResult));
                    return Problem(argException.ToString(), statusCode: StatusCodes.Status500InternalServerError,
                        title: argException.Message);
                }
            }
        }
    }

    public class ObjectIdValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) => value is string strVal
            ? ObjectId.TryParse(strVal, out var _)
                ? ValidationResult.Success
                : new ValidationResult("Not a valid ObjectId.")
            : new ValidationResult("Input must be a string.");
    }

    public class SendInDto
    {
        [EmailAddressRex]
        public string From { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> ToRecipients { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> CcRecipients { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> BccRecipients { get; set; }

        public dynamic Data { get; set; }
    }

    public class TemplateRevisionInDto
    {
        public string SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; }

        [EmailAddressRex]
        public string DefaultFrom { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> DefaultToRecipients { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> DefaultCcRecipients { get; set; }

        [EmailAddressRex]
        public IEnumerable<string> DefaultBccRecipients { get; set; }
    }

    public class TemplateInDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}