using System;
using System.IO;
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
            StartAppInt();
            GetMaindetails("Annecy");
        }

        //Init du fichier de recherche
        private void StartAppInt()
        {
            List<string> unitList = new List<string>();
            using (var MyFile = File.OpenText(@"Assets/villes_france.txt"))
            {
                string txtcontent = MyFile.ReadLine();
                int linenb = 0;
                do
                {
                    txtcontent = MyFile.ReadLine();
                    unitList.Add(txtcontent);
                    linenb++;
                } while (txtcontent != null);
            }
            CB_City.ItemsSource = unitList;
        }

        public async Task GetMaindetails(string Ville)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://www.prevision-meteo.ch/services/json/" + Ville);
            if (response.IsSuccessStatusCode)
            {
                CB_City.SelectedItem = Ville;

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

                string filePath = @"Assets/villes_france.txt";


                if (cityinfo == null || currentCondition == null || fcstDay0 == null || fcstDay1 == null || fcstDay2 == null || fcstDay3 == null)
                {
 
                    delete(filePath, Ville);
                    MessageBox.Show("Les données reçues sont incomplètes.");
                    return;
                }
                //declaration des variables API
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

                //Chargement des images
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

                //Affichage des données
                TB_dayA.Text = dayA;
                TB_dayB.Text = dayB;
                TB_dayC.Text = dayC;

                TB_tempA.Text = tmax1.ToString() + "°/" + tmin1.ToString() + '°';
                TB_tempB.Text = tmax2.ToString() + "°/" + tmin2.ToString() + '°';
                TB_tempC.Text = tmax3.ToString() + "°/" + tmin3.ToString() + '°';

                TB_temp.Text = currentTemp.ToString() + '°';
                TB_desc.Text = currentCondition.condition.ToUpper();
                TB_dif.Text = tmax.ToString() + "°/" + tmin.ToString() + '°';
            }
            else
            {
                MessageBox.Show("La requête HTTP a échoué.");
            }
        }

        //Event quand on change de ville
        private void CB_City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_City.SelectedItem == null)
            {
                GetMaindetails("Annecy");
            }
            else
            {
                GetMaindetails(CB_City.SelectedItem.ToString());
            }
          
        }

        //Event pour le bouton d'ajout
        private void BTN_Add(object sender, RoutedEventArgs e)
        {
            string newCity = TB_Add.Text;
            if (!string.IsNullOrEmpty(newCity))
            {
                try
                {
                    // Vérifier si la ville existe dans le fichier cleaned_city.txt
                    string cleanedCityFilePath = @"Assets/cleaned_city_names.txt";
                    var cleanedCities = File.ReadAllLines(cleanedCityFilePath).ToList();
                    newCity = CapitalizeFirstLetter(newCity.ToLower());
                    if (!cleanedCities.Contains(newCity))
                    {
                        MessageBox.Show("La ville n'existe pas, veuillez reesayer une autre ville", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string filePath = @"Assets/villes_france.txt";
                    File.AppendAllText(filePath, newCity);
                    MessageBox.Show($"La ville '{newCity}' a été ajoutée à la liste.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    UpdateComboBox();
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez entrer le nom d'une ville.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Mettre à jour la ComboBox
        private void UpdateComboBox()
        {
            try
            {
                string filePath = @"Assets/villes_france.txt";
                var cities = File.ReadAllLines(filePath).ToList();
                CB_City.ItemsSource = cities;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur s'est produite lors de la mise à jour de la ComboBox : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        //Event pour le bouton de suppression
        private void BTN_Del(object sender, RoutedEventArgs e)
        {
            if (CB_City.SelectedItem != null)
            {
                string selectedCity = CB_City.SelectedItem.ToString();
                string filePath = @"Assets/villes_france.txt";
               
                try
                {
                    if (delete(filePath, selectedCity) == true)
                    {
                        MessageBox.Show($"La ville '{selectedCity}' a été supprimée de la liste.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("La ville sélectionnée n'existe pas dans la liste.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une ville à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        //Fonction pour supprimer une ville de la liste
        private bool delete(string file, string city)
        {
            var lines = File.ReadAllLines(file).ToList();

            // Supprimer la ville sélectionnée de la liste
            if (lines.Contains(city))
            {
                lines.Remove(city);

                // Écrire les lignes mises à jour dans le fichier
                File.WriteAllLines(file, lines);

                // Mettre à jour la ComboBox
                UpdateComboBox();
                return true;
            }
            return false;
            
        }
        //Fonction pour mettre la 1er lettre en majuscule
        string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input; // Retourner tel quel si la chaîne est vide ou null

            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
    

}