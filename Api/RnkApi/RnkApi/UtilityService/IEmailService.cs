using login.model.Dto;
using System;

namespace login.UtilityService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel);
             
    }
}
