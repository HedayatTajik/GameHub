
using Microsoft.AspNetCore.Components;
using System.Net.NetworkInformation;


namespace ImposterGame.Pages
{
    public partial class Timer : ComponentBase
    {
        [Parameter] [SupplyParameterFromQuery] public int PlayerCount { get; set; } = 3;

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        public int timeLeft = 180;
        public bool isRunning = false;

        public string formattedTime => $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
        protected override void OnInitialized()
        {
            timeLeft = 60 * PlayerCount;
            _ = StartTimer();
        }
        public async Task StartTimer()
        {
            if (isRunning)
                return;

            isRunning = true;

            while (timeLeft > 0)
            {
                await Task.Delay(1000);
                timeLeft--;
                await InvokeAsync(StateHasChanged);
            }

            isRunning = false;
        }

        public void StartAgain()
        {
            NavigationManager.NavigateTo($"/impostergame");
        }
    }
}