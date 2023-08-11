using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Logic;
using DataAccess.Models;
using Microsoft.Extensions.Hosting;

namespace Services.BackgroundServices
{
    public class AccountTokenExpirationService : BackgroundService
    {
        private readonly IUserDataAccess _userDataAccess;
        public AccountTokenExpirationService(IUserDataAccess userDataAccess)
        {
            _userDataAccess = userDataAccess;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<TokenDataModel> tokens = _userDataAccess.GetTokens();
                foreach (TokenDataModel token in tokens)
                {
                    if(token.TokenExpiration < DateTime.Now && token.IsActivationToken)
                    {
                        _userDataAccess.DeleteAccountActivationTokenAndUser(token.Token, token.UserOfToken);
                    }
                    else if(token.TokenExpiration < DateTime.Now)
                    {
                        _userDataAccess.DeleteToken(token.Token);
                    }
                }

                // Interval of 30 minutes
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); 
            }
        }
    }
}
