﻿using BayardsSafetyApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BayardsSafetyApp
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            AInd.IsEnabled = false;
            AInd.IsRunning = false;
            BackgroundColor = Color.FromHex("#efefef");
            
        }

        bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string password = string.Empty;
        private async void ContinueButton_Clicked(object sender, EventArgs e)
        {
            AInd.IsEnabled = true;
            AInd.IsRunning = true;
            ContinueButton.IsEnabled = false;
            var AllSections = new Sections();
            API api = new API();
            if (api.CheckInternetConnection())
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        if (api.isPasswordCorrect(PasswordEntry.Text))
                        {
                            if (Application.Current.Properties.ContainsKey("LocAgr") && (bool)Application.Current.Properties["LocAgr"])
                            {
                                AllSections.Contents = await LoadSections();
                                throw new Exception("1");
                            }

                            else
                                throw new Exception("2");
                        }
                        else
                        {
                            throw new Exception("Incorrect");
                        }
                    });

                }
                catch (TaskCanceledException)
                {
                    await DisplayAlert("Warning", "The server doesn't respond", "OK");
                }
                catch (ArgumentException)
                {

                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("Incorrect"))
                        await DisplayAlert("Warning", "The password is incorrect", "OK");
                    if (ex.Message.StartsWith("1"))
                        await Navigation.PushAsync(AllSections);
                    if (ex.Message.StartsWith("2"))
                        await Navigation.PushAsync(new LocalePage());
                    if (ex.Message.StartsWith("3"))
                        if (await DisplayAlert("Warning",
                        AppResources.LangResources.DownloadWarn,
                        "OK", "Cancel"))
                            await Navigation.PushAsync(new LoadingDataPage());
                }
            }
            else
            {
                await DisplayAlert("Warning", "The app needs internet connection to check the password", "OK");
            }
            AInd.IsEnabled = false;
            AInd.IsRunning = false;
            ContinueButton.IsEnabled = true;

        }

        private void PasswordEntry_Completed(object sender, EventArgs e)
        {
            password = ((Entry)sender).Text;
        }

        private async Task<List<Section>> LoadSections()
        {
            API api = new API();
            List<Section> contents = new List<Section>();
            if (!Application.Current.Properties.ContainsKey("UpdateTime") || 
                !(Application.Current.Properties.ContainsKey("AllSections")&& Application.Current.Properties.ContainsKey("AllRisks"))||
                api.isUpdataNeeded((DateTime)Application.Current.Properties["UpdateTime"]).Result)
            {
                    throw new Exception("3");
            }
            else
            {
                //contents = App.Database.SectionDatabase.GetItems<Section>().ToList().FindAll(s => s.Parent_s == "null"
                //                                                                        && s.Lang == AppResources.LangResources.Language).
                //                                                                        OrderBy(s => s.Name).ToList();
                contents = Utils.DeserializeFromJson<List<Section>>((string)Application.Current.Properties["AllSections"]).
                    FindAll(s => s.Parent_s == "null" && s.Lang == AppResources.LangResources.Language).OrderBy(s => s.Name).ToList();
            }

            return contents;
        }
    }
}
