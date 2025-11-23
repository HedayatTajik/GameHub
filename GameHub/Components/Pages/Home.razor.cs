using Microsoft.AspNetCore.Components;

namespace GameHub.Components.Pages
{
    public partial class Home
    {
    [Inject] private NavigationManager Navigation { get; set; } = default!;
        private void GoToGame()
        {
            Navigation.NavigateTo("/impostergame");
        }
        private void GoToAbout()
        {
            Navigation.NavigateTo("/aboutgame");
        }

    }
}