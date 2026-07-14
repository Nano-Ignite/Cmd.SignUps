using System;
using System.Threading;
using System.Threading.Tasks;
using DynamicExpression.Entities;
using Microsoft.Extensions.Logging;
using Nano.App.Console.Workers;
using Svc.Accounts.Models.Api;
using Svc.Accounts.Models.Criterias;
using Svc.Emailing.Models.Api;
using Svc.Emailing.Models.Data;
using Svc.Emailing.Models.Data.Enums;
using User = Svc.Accounts.Models.Data.User;

namespace Cmd.SignUps.Workers;

/// <inheritdoc/>
public class NewUserSignUpWorker(ILogger<NewUserSignUpWorker> logger, AccountsApi accountsApi, EmailingApi emailingApi) 
    : BaseWorker(logger)
{
    /// <inheritdoc />
    public override async Task OnStartAsync(CancellationToken cancellationToken = default)
    {
        var users = await accountsApi.Entity
            .QueryAsync<User, UserQueryCriteria>(new Query<UserQueryCriteria>
            {
                Criteria =
                {
                    CreatedAfter = DateTimeOffset.UtcNow.AddHours(-2)    
                },
                Paging =
                {
                    Count = 25
                }
            }, cancellationToken);

        foreach (var user in users)
        {
            await emailingApi
                .SendEmailAsync(new Email
                {
                    UserId = user.Id,
                    Type = EmailType.Welcome,
                    Data = new
                    {
                        Username = user.FullName
                    }
                }, cancellationToken);
        }
    }
}