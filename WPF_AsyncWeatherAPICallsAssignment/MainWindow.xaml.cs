using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Newtonsoft.Json;
using WPF_AsyncWeatherAPICallsAssignment.Models;

namespace WPF_AsyncWeatherAPICallsAssignment
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "e80758081f2bf960047caffb2c97ab3a";
        private const string CurrentWeatherUrl =
            "http://api.openweathermap.org/data/2.5/weather?" +
            "@QUERY@=@LOC@&units=imperial&APPID=" +
            API_KEY;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void getForecastSyncButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear the current window contents
            forecasts.Text = "";
            elapsedTime.Text = "";

            //Start a stopwatch to time how long this takes
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            //Split the zipcodes entered by comma
            string[] zips = zipCodes.Text.Split(',');
            GetWeather(zips);

            //Stop the stopwatch and get how many milliseconds this took
            stopWatch.Stop();
            var elapsedMs = stopWatch.ElapsedMilliseconds;

            elapsedTime.Text += $"Total execution time: {elapsedMs}.";
        }

        private async void getForecastAsyncButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear the current window contents
            forecasts.Text = "";
            elapsedTime.Text = "";

            //Start a stopwatch to time how long this takes
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            //Split the zipcodes entered by comma
            string[] zips = zipCodes.Text.Split(',');
            await GetWeatherAsync(zips);

            //Stop the stopwatch and get how many milliseconds this took
            stopWatch.Stop();
            var elapsedMs = stopWatch.ElapsedMilliseconds;

            elapsedTime.Text += $"Total execution time: {elapsedMs}.";
        }

        private void GetWeather(string[] zipCodes)
        {
            foreach(string zip in zipCodes)
            {
                ForecastModel fcm = GetForecastByZip(zip.Trim());

                forecasts.Text += $"Weather for {fcm.name}: {fcm.weather[0].description}.";
                forecasts.Text += Environment.NewLine;
            }
        }

        private ForecastModel GetForecastByZip(string zip)
        {
            //Sample API Call:
            //  api.openweathermap.org/data/2.5/weather?zip=94040,us          

            //Format our URL to tell it we are searching by zip and pass the zip
            string url = CurrentWeatherUrl.Replace("@QUERY@", "zip");
            url = url.Replace("@LOC@", zip);

            //Wrap a using around our WebClient since it implements iDisposable and we don't want it hanging around
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    //Call the Open Weather API and save the result
                    string returnedJson = webClient.DownloadString(url);
                    //Get the JSON into our model
                    ForecastModel forecastModel = JsonConvert.DeserializeObject<ForecastModel>(returnedJson);
                    return forecastModel;

                }
                catch (Exception ex)
                {
                    //If something goes wrong, throw an exception
                    throw new Exception($"Error encountered during API read: {ex.Message}.");
                }
            }
        }

        private async Task GetWeatherAsync(string[] zipCodes)
        {
            //Create a list of Tasks
            List<Task<ForecastModel>> taskList = new List<Task<ForecastModel>>();

            //Create and start up a task for each zip in our list
            foreach(string zip in zipCodes)
            {
                taskList.Add(Task.Run(() => GetForecastByZipAsync(zip)));
            }

            //Now, when all the Tasks in the taskList are complete, populate them into an array[] of our ForecastModel
            ForecastModel[] forecastArray = await Task.WhenAll(taskList);

            //Now loop through the Forecast Model array and write the forecast to our form
            foreach (ForecastModel forecast in forecastArray)
            {
                forecasts.Text += $"Weather for {forecast.name}: {forecast.weather[0].description}.";
                forecasts.Text += Environment.NewLine;
            }
        }

        private async Task<ForecastModel> GetForecastByZipAsync(string zip)
        {
            //Sample API Call:
            //  api.openweathermap.org/data/2.5/weather?zip=94040,us          

            //Format our URL to tell it we are searching by zip and pass the zip
            string url = CurrentWeatherUrl.Replace("@QUERY@", "zip");
            url = url.Replace("@LOC@", zip);

            //Wrap a using around our WebClient since it implements iDisposable and we don't want it hanging around
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    //Call the Open Weather API and save the result                 
                    string returnedJson = await Task.Run(() => webClient.DownloadString(url));
                    //Get the JSON into our model
                    ForecastModel forecastModel = JsonConvert.DeserializeObject<ForecastModel>(returnedJson);
                    return forecastModel;

                }
                catch (Exception ex)
                {
                    //If something goes wrong, throw an exception
                    throw new Exception($"Error encountered during API read: {ex.Message}.");
                }
            }
        }
    }
}
