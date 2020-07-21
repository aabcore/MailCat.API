using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MailCat.API.Controllers;
using MailCat.API.Models;
using MailCat.API.Models.Entities;
using MailCat.API.Models.Filters;
using MailCat.API.Results;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MailCat.API.Services
{
    public class TemplateService : DbService
    {
        private IMapper Mapper { get; }
        private readonly IMongoCollection<TemplateEntity> _templateCollection;
        private readonly IMongoCollection<TemplateRevisionEntity> _templateRevisionCollection;

        public TemplateService(DatabaseSettings dbSettings, IMapper mapper)
        {
            Mapper = mapper;
            var database = BuildDatabaseClient(dbSettings);
            _templateCollection = database.GetCollection<TemplateEntity>(dbSettings.TemplateCollectionName);
            _templateRevisionCollection = database.GetCollection<TemplateRevisionEntity>(dbSettings.TemplateRevisionCollectionName);
        }

        public async Task<TypedResult<TemplateEntity>> NewTemplate(TemplateInDto templateIn)
        {
            try
            {
                var newTemplate = Mapper.Map<TemplateInDto, TemplateEntity>(templateIn);
                await _templateCollection.InsertOneAsync(newTemplate);

                return new SuccessfulTypedResult<TemplateEntity>(newTemplate);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<TemplateEntity>(e);
            }
        }

        public async Task<TypedResult<IEnumerable<TemplateEntity>>> GetTemplates(GetTemplatesFilter getTemplatesFilter)
        {
            if (getTemplatesFilter == null)
            {
                return new FailedTypedResult<IEnumerable<TemplateEntity>>(new ArgumentNullException(nameof(getTemplatesFilter)));
            }

            var filters = new List<FilterDefinition<TemplateEntity>>();

            if (!string.IsNullOrWhiteSpace(getTemplatesFilter.NameContains))
            {
                filters.Add(Builders<TemplateEntity>.Filter.Regex(te => te.Name, getTemplatesFilter.NameContains));
            }
            if (!string.IsNullOrWhiteSpace(getTemplatesFilter.DescriptionContains))
            {
                filters.Add(Builders<TemplateEntity>.Filter.Regex(te => te.Description, getTemplatesFilter.DescriptionContains));
            }
            if (getTemplatesFilter.DateBefore.HasValue)
            {
                filters.Add(Builders<TemplateEntity>.Filter.Lte(te => te.CreatedDate, getTemplatesFilter.DateBefore));
            }


            var opts = new FindOptions<TemplateEntity>()
                { Limit = getTemplatesFilter.Limit, Skip = getTemplatesFilter?.Skip, Sort = Builders<TemplateEntity>.Sort.Descending(me => me.CreatedDate) };

            if (filters.Any())
            {
                var foundTemplates = (await _templateCollection.FindAsync(Builders<TemplateEntity>.Filter.And(filters), opts)).ToList();
                return new SuccessfulTypedResult<IEnumerable<TemplateEntity>>(foundTemplates);
            }
            else
            {
                var foundTemplates = (await _templateCollection.FindAsync(Builders<TemplateEntity>.Filter.Empty, opts)).ToList();
                return new SuccessfulTypedResult<IEnumerable<TemplateEntity>>(foundTemplates);
            }

        }


        public async Task<TypedResult<TemplateRevisionEntity>> NewTemplateRevision(ObjectId templateId, TemplateRevisionInDto templateRevisionIn)
        {
            try
            {
                var foundTemplate = (await _templateCollection.FindAsync(t => t.Id == templateId)).FirstOrDefault();
                if (foundTemplate == null)
                {
                    return new BadRequestTypedResult<TemplateRevisionEntity>("templateId", "No matching template found.");
                }

                var latestTemplateRevisions = (await _templateRevisionCollection.FindAsync(tr => tr.TemplateReference == templateId,
                    new FindOptions<TemplateRevisionEntity>()
                    {
                        Sort = Builders<TemplateRevisionEntity>.Sort.Descending(tr => tr.RevisionNumber),
                        Limit = 1
                    })).FirstOrDefault();

                var newTemplateRevision = Mapper.Map<TemplateRevisionInDto, TemplateRevisionEntity>(templateRevisionIn);
                newTemplateRevision.CreatedDate = DateTimeOffset.UtcNow;
                newTemplateRevision.TemplateReference = templateId;
                newTemplateRevision.RevisionNumber = (latestTemplateRevisions?.RevisionNumber ?? 0) + 1;

                await _templateRevisionCollection.InsertOneAsync(newTemplateRevision);
                return new SuccessfulTypedResult<TemplateRevisionEntity>(newTemplateRevision);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<TemplateRevisionEntity>(e);
            }
        }

        public async Task<TypedResult<TemplateEntity>> GetTemplate(ObjectId templateId)
        {
            try
            {
                var foundTemplate = (await _templateCollection.FindAsync(t => t.Id == templateId)).FirstOrDefault();
                if (foundTemplate == null)
                {
                    return new NotFoundTypedResult<TemplateEntity>();
                }

                return new SuccessfulTypedResult<TemplateEntity>(foundTemplate);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<TemplateEntity>(e);
            }
        }

        public async Task<TypedResult<TemplateRevisionEntity>> GetTemplateRevision(ObjectId templateId, ObjectId templateRevisionId)
        {
            try
            {
                var foundTemplateRevision = (await _templateRevisionCollection.FindAsync(t => t.Id == templateRevisionId && t.TemplateReference == templateId)).FirstOrDefault();
                if (foundTemplateRevision == null)
                {
                    return new NotFoundTypedResult<TemplateRevisionEntity>();
                }

                return new SuccessfulTypedResult<TemplateRevisionEntity>(foundTemplateRevision);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<TemplateRevisionEntity>(e);
            }
        }

        public async Task<TypedResult<TemplateEntity>> UpdateTemplate(ObjectId templateId, TemplateInDto templateIn)
        {
            try
            {
                var updates = new List<UpdateDefinition<TemplateEntity>>();
                if (!string.IsNullOrWhiteSpace(templateIn?.Name))
                {
                    updates.Add(Builders<TemplateEntity>.Update.Set(te => te.Name, templateIn.Name));
                }

                if (!string.IsNullOrWhiteSpace(templateIn?.Description))
                {
                    updates.Add(Builders<TemplateEntity>.Update.Set(te => te.Description, templateIn.Description));
                }

                if (!updates.Any())
                {
                    return new BadRequestTypedResult<TemplateEntity>("templateIn", "No updates to apply.");
                }

                var foundTemplate = await _templateCollection.FindOneAndUpdateAsync<TemplateEntity>(t => t.Id == templateId,
                    Builders<TemplateEntity>.Update.Combine(updates),
                    GetEntityAfterUpdateOption<TemplateEntity>());

                if (foundTemplate == null)
                {
                    return new NotFoundTypedResult<TemplateEntity>();
                }

                return new SuccessfulTypedResult<TemplateEntity>(foundTemplate);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<TemplateEntity>(e);
            }
        }

        public async Task<TypedResult<IEnumerable<TemplateRevisionEntity>>> GetFilteredTemplateRevisions(ObjectId templateId, GetTemplateRevisionsFilter getTemplateRevisionsFilter)
        {
            try
            {
                if (getTemplateRevisionsFilter == null)
                {
                    return new BadRequestTypedResult<IEnumerable<TemplateRevisionEntity>>("TemplateRevisionsFilter", "TemplateRevisionsFilter must not be null.");
                }

                var filters = new List<FilterDefinition<TemplateRevisionEntity>>();
                if (getTemplateRevisionsFilter.DateBefore.HasValue)
                {
                    filters.Add(Builders<TemplateRevisionEntity>.Filter.Lte(te => te.CreatedDate, getTemplateRevisionsFilter.DateBefore));
                }


                var opts = new FindOptions<TemplateRevisionEntity>()
                    { Limit = getTemplateRevisionsFilter.Limit, Skip = getTemplateRevisionsFilter.Skip, Sort = Builders<TemplateRevisionEntity>.Sort.Descending(me => me.CreatedDate) };
                if (filters.Any())
                {
                    var foundTemplates = (await _templateRevisionCollection.FindAsync(Builders<TemplateRevisionEntity>.Filter.And(filters), opts)).ToList();
                    return new SuccessfulTypedResult<IEnumerable<TemplateRevisionEntity>>(foundTemplates);
                }
                else
                {
                    var foundTemplates = (await _templateRevisionCollection.FindAsync(Builders<TemplateRevisionEntity>.Filter.Empty, opts)).ToList();
                    return new SuccessfulTypedResult<IEnumerable<TemplateRevisionEntity>>(foundTemplates);
                }
            }
            catch (Exception e)
            {
                return new FailedTypedResult<IEnumerable<TemplateRevisionEntity>>(e);
            }
        }

        public async Task<TypedResult<TemplateRevisionEntity>> GetLatestTemplateRevision(ObjectId templateId)
        {
            try
            {
                var foundTemplateRevision = (await _templateRevisionCollection.FindAsync(tr => tr.TemplateReference == templateId,
                    new FindOptions<TemplateRevisionEntity>()
                    {
                        Limit = 1, Sort = Builders<TemplateRevisionEntity>.Sort.Descending(tre => tre.RevisionNumber)
                    })).FirstOrDefault();

                if (foundTemplateRevision == null)
                {
                    return new NotFoundTypedResult<TemplateRevisionEntity>();
                }

                return new SuccessfulTypedResult<TemplateRevisionEntity>(foundTemplateRevision);
            }
            catch (Exception e)
            {
                return new FailedTypedResult<TemplateRevisionEntity>(e);
            }
        }
    }
}