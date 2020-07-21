using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MailCat.API.Controllers;
using MailCat.API.Models.ApiDtos;
using MailCat.API.Models.Entities;

namespace MailCat.API
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<NewEmailInDto, MailEntity>()
                .ForMember(dest => dest.CcRecipients, opt => opt.AllowNull())
                .ForMember(dest => dest.BccRecipients, opt => opt.AllowNull());
            CreateMap<MailEntity, MailOutDto>();
            CreateMap<TemplateInDto, TemplateEntity>();
            CreateMap<TemplateRevisionInDto, TemplateRevisionEntity>()
                .ForMember(dest => dest.DefaultToRecipients, opt => opt.AllowNull())
                .ForMember(dest => dest.DefaultCcRecipients, opt => opt.AllowNull())
                .ForMember(dest => dest.DefaultBccRecipients, opt => opt.AllowNull());
            CreateMap<TemplateEntity, TemplateOutDto>();
            CreateMap<TemplateRevisionEntity, TemplateRevisionOutDto>()
                .ForMember(dest => dest.DefaultToRecipients, opt => opt.AllowNull())
                .ForMember(dest => dest.DefaultCcRecipients, opt => opt.AllowNull())
                .ForMember(dest => dest.DefaultBccRecipients, opt => opt.AllowNull());
        }
    }
}
