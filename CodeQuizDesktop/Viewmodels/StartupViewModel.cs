using CodeQuizDesktop.Repositories;
using CodeQuizDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Viewmodels
{
    public class StartupViewModel : BaseViewModel
    {
        private readonly ITokenService tokenService;
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly IUsersRepository usersRepository;
        public StartupViewModel(ITokenService tokenService, IAuthenticationRepository authenticationRepository, IUsersRepository usersRepository)
        {
            this.tokenService = tokenService;
            this.authenticationRepository = authenticationRepository;
            this.usersRepository = usersRepository;
            Initialize();
        }

        public async void Initialize()
        {
            await Task.Delay(2000);
            var token = await tokenService.GetValidTokens();
            if (token != null)
            {
                authenticationRepository.LoggedInUser = await usersRepository.GetUser();
                await Shell.Current.GoToAsync("///MainPage");
            }
            else
            {
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }
    }
}
