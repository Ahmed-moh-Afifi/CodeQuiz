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
        public StartupViewModel(ITokenService tokenService)
        {
            this.tokenService = tokenService;
            Initialize();
        }

        public async void Initialize()
        {
            await Task.Delay(2000);
            var token = await tokenService.GetValidTokens();
            if (token != null)
            {
                await Shell.Current.GoToAsync("///MainPage");
            }
            else
            {
                await Shell.Current.GoToAsync("///LoginPage");
            }
        }
    }
}
