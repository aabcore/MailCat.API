using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MailCat.API.Controllers;
using MailCat.API.Models;
using MailCat.API.Models.Entities;
using MailCat.API.Models.Filters;
using MailCat.API.Results;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MailCat.API.Services
{
    public class MailService : DbService
    {
        private IMapper Mapper { get; }
        private readonly IMongoCollection<MailEntity> _mailCollection;

        public MailService(DatabaseSettings dbSettings, IMapper mapper, IConfiguration config): base(config)
        {
            Mapper = mapper;
            var database = BuildDatabaseClient(dbSettings);
            _mailCollection = database.GetCollection<MailEntity>(dbSettings.MailCollectionName);
        }

        public async Task<TypedResult<IEnumerable<MailEntity>>> GetMail(GetMailFilter getMailFilter)
        {
            try
            {
                var filters = new List<FilterDefinition<MailEntity>>();

                if (getMailFilter?.After != null)
                {
                    filters.Add(Builders<MailEntity>.Filter.Gte(me => me.Date, getMailFilter.After));
                }

                if (getMailFilter?.Before != null)
                {
                    filters.Add(Builders<MailEntity>.Filter.Lte(me => me.Date, getMailFilter.Before));
                }

                if (!string.IsNullOrWhiteSpace(getMailFilter?.ToEmail))
                {
                    var emailFilter = new List<FilterDefinition<MailEntity>>
                    {
                        Builders<MailEntity>.Filter.AnyEq(me => me.ToRecipients, getMailFilter.ToEmail),
                        Builders<MailEntity>.Filter.AnyEq(me => me.CcRecipients, getMailFilter.ToEmail),
                        Builders<MailEntity>.Filter.AnyEq(me => me.BccRecipients, getMailFilter.ToEmail)
                    };
                    filters.Add(Builders<MailEntity>.Filter.Or(emailFilter));
                }

                if (!string.IsNullOrWhiteSpace(getMailFilter?.FromEmail))
                {
                    filters.Add(Builders<MailEntity>.Filter.Eq(me => me.From, getMailFilter.FromEmail));
                }

                var opts = new FindOptions<MailEntity>()
                    {Limit = getMailFilter.Limit, Skip = getMailFilter.Skip, Sort = Builders<MailEntity>.Sort.Descending(me => me.Date)};

                if (filters.Any())
                {
                    var foundMail = (await _mailCollection.FindAsync(Builders<MailEntity>.Filter.And(filters), opts)).ToList();
                    return new SuccessfulTypedResult<IEnumerable<MailEntity>>(foundMail);
                }
                else
                {
                    var foundMail = (await _mailCollection.FindAsync(Builders<MailEntity>.Filter.Empty, opts)).ToList();
                    return new SuccessfulTypedResult<IEnumerable<MailEntity>>(foundMail);
                }
            }
            catch (Exception e)
            {
                return new FailedTypedResult<IEnumerable<MailEntity>>(e);
            }
        }

        public async Task<TypedResult<bool>> CreateMail(NewEmailInDto newMailIn)
        {
            try
            {
                var newMailEntity = Mapper.Map<NewEmailInDto, MailEntity>(newMailIn);
                newMailEntity.Date = DateTimeOffset.UtcNow;

                await _mailCollection.InsertOneAsync(newMailEntity);
                return new SuccessfulTypedResult<bool>(true);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<bool>(e);
            }
        }
    }
}