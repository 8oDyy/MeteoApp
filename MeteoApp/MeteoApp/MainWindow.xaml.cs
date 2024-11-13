using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json;


namespace MeteoApp
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GetMaindetails();
        }

        public async Task GetMaindetails()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://www.prevision-meteo.ch/services/json/Annecy");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show("Le contenu de la réponse est vide.");
                    return;
                }

                Root root;
                try
                {
                    root = JsonConvert.DeserializeObject<Root>(content);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur de désérialisation : {ex.Message}");
                    return;
                }
                CityInfo cityinfo = root.city_info;
                CurrentCondition currentCondition = root.current_condition;
                FcstDay0 fcstDay0 = root.fcst_day_0;
                FcstDay1 fcstDay1 = root.fcst_day_1;
                FcstDay2 fcstDay2 = root.fcst_day_2;
                FcstDay3 fcstDay3 = root.fcst_day_3;

                var name = cityinfo.name.ToUpper();
                var tmax = fcstDay0.tmax;
                var tmin = fcstDay0.tmin;
                var currentTemp = currentCondition.tmp;
                var dayA = fcstDay1.day_long;
                var dayB = fcstDay2.day_long;
                var dayC = fcstDay3.day_long;
                var tmin1 = fcstDay1.tmin;
                var tmax1 = fcstDay1.tmax;
                var tmin2 = fcstDay2.tmin;
                var tmax2 = fcstDay2.tmax;
                var tmin3 = fcstDay3.tmin;
                var tmax3 = fcstDay3.tmax;

                string icon = fcstDay0.icon_big;
                Uri ressource = new Uri(icon);
                ImgMeteo.Source = new BitmapImage(ressource);
                string icon1 = fcstDay1.icon;
                Uri ressource1 = new Uri(icon1);
                ImgDayA.Source = new BitmapImage(ressource1);
                string icon2 = fcstDay2.icon;
                Uri ressource2 = new Uri(icon2);
                ImgDayB.Source = new BitmapImage(ressource2);
                string icon3 = fcstDay3.icon;
                Uri ressource3 = new Uri(icon3);
                ImgDayC.Source = new BitmapImage(ressource3);


                TB_city.Text = name;
                TB_dayA.Text = dayA;
                TB_dayB.Text = dayB;
                TB_dayC.Text = dayC;
                TB_temp.Text = currentTemp.ToString() + '°';
                TB_desc.Text = currentCondition.condition.ToUpper();
                TB_dif.Text = tmax.ToString() + "°/" + tmin.ToString() + '°';
            }
            else
            {
                MessageBox.Show("La requête HTTP a échoué.");
            }
        }
    }

}