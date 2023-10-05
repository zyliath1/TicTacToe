using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace final_work
{
    public partial class MainPage : ContentPage
    {
        private readonly PelaajaHallinta _pelaajaHallinta;
        public ObservableCollection<string> PelaajatNimet { get; set; } = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();

            var tallennusPalvelu = new TiedostoPelaajanTallennus();
            _pelaajaHallinta = new PelaajaHallinta(tallennusPalvelu);

            this.BindingContext = this;
            PelaajatNimet.Clear();

            foreach (var pelaaja in _pelaajaHallinta.LataaPelaajat())
            {
                if (pelaaja.Etunimi != "Tietokone")
                {
                    PelaajatNimet.Add($"{pelaaja.Etunimi} {pelaaja.Sukunimi}");
                }
            }
        }

        public static readonly Pelaaja TietokonePelaaja = new Pelaaja
        {
            Etunimi = "Tietokone",
            PelaajaId = Guid.Empty
        };

        private async void AloitaPeliClicked(object sender, EventArgs e)
        {
            if (PelaajaPicker.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse pelaaja ennen pelin aloittamista.", "OK");
                return;
            }

            var selectedPlayerName = PelaajaPicker.SelectedItem.ToString();
            var nameParts = selectedPlayerName.Split(' ');
            if (nameParts.Length != 2)
            {
                await DisplayAlert("Virhe", "Virheellinen nimi.", "OK");
                return;
            }
            var selectedPlayer = _pelaajaHallinta.HaePelaaja(nameParts[0], nameParts[1]);


            Pelaaja vastustaja;

            if (TietokoneRadio.IsChecked)
            {
                vastustaja = TietokonePelaaja;
            }

            else
            {
                if (VastustajaPicker.SelectedItem == null)
                {
                    await DisplayAlert("Virhe", "Valitse vastustaja.", "OK");
                    return;
                }

                var selectedOpponentName = VastustajaPicker.SelectedItem.ToString();
                var opponentNameParts = selectedOpponentName.Split(' ');
                if (opponentNameParts.Length != 2)
                {
                    await DisplayAlert("Virhe", "Virheellinen nimi.", "OK");
                    return;
                }
                vastustaja = _pelaajaHallinta.HaePelaaja(opponentNameParts[0], opponentNameParts[1]);

                // Käytetään Id:tä vertailussa, koska muuten ei toimi
                if (selectedPlayer.PelaajaId == vastustaja.PelaajaId)
                {
                    await DisplayAlert("Virhe", "Et voi pelata itseäsi vastaan.", "OK");
                    return;
                }
            }

            await Navigation.PushAsync(new PeliSivu(selectedPlayer, vastustaja));
        }


        private void OnVastustajaValintaChanged(object sender, CheckedChangedEventArgs e)
        {
            VastustajaPicker.IsVisible = ToinenPelaajaRadio.IsChecked;
            
            if (TietokoneRadio.IsChecked == true)
            {
                TallennaVastustajaJsoniin("tietokone");
            }
        }

        private void TallennaVastustajaJsoniin(string vastustaja)
        {
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string jsonFilePath = Path.Combine(userFolderPath, "pelaajat.json");
            JToken token;

            // Load the current JSON file (if it exists)
            if (File.Exists(jsonFilePath))
            {
                using (StreamReader file = File.OpenText(jsonFilePath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    token = JToken.ReadFrom(reader);
                }
            }
            else
            {
                token = new JObject();
            }

            // If the content is a JObject
            if (token is JObject json)
            {
                // Set or update the opponent
                json["vastustaja"] = vastustaja;
            }
            else if (token is JArray jsonArray)
            {
                // If the content is a JArray
                // Handle this case based on your requirements.
                // For example, you can create a new JObject and add it to the JArray.
                var newObject = new JObject();
                newObject["vastustaja"] = vastustaja;
                jsonArray.Add(newObject);
                token = jsonArray; // Use the updated JArray as the token to write back to the file.
            }
            else
            {
                // Handle other types or throw an exception
                throw new InvalidDataException("Unexpected JSON token type.");
            }

            // Save the updated JSON file
            File.WriteAllText(jsonFilePath, token.ToString());
        }


        private async void TallennaTiedotClicked(object sender, EventArgs e)
        {
            string etunimi = EtunimiEntry.Text;
            string sukunimi = SukunimiEntry.Text;

            if (!int.TryParse(SyntymaVuosiEntry.Text, out int syntymavuosi))
            {
                await DisplayAlert("Virhe", "Syötä kelvollinen syntymävuosi!", "OK");
                return;
            }

            // Tarkista onko pelaaja jo listalla
            var olemassaOlevaPelaaja = _pelaajaHallinta.HaePelaaja(etunimi, sukunimi);

            if (olemassaOlevaPelaaja != null)
            {
                await DisplayAlert("Virhe", "Samanniminen pelaaja on jo olemassa!", "OK");
                return;
            }

            var uusiPelaaja = new Pelaaja
            {
                Etunimi = etunimi,
                Sukunimi = sukunimi,
                Syntymävuosi = syntymavuosi,
                Voitot = 0,
                Tappiot = 0,
                Tasapelit = 0,
                PelienYhteiskesto = TimeSpan.Zero
            };

            TallennaPelaaja(uusiPelaaja);
            EtunimiEntry.Text = "";
            SukunimiEntry.Text = "";
            SyntymaVuosiEntry.Text = "";
            await DisplayAlert("Vahvistus", "Tiedot tallennettu onnistuneesti!", "OK");
        }

        // Käytä tätä metodia kun haluat tallentaa tai päivittää pelaajan
        private void TallennaPelaaja(Pelaaja pelaaja)
        {
            try
            {
                string kokonimi = $"{pelaaja.Etunimi} {pelaaja.Sukunimi}";
                bool success = _pelaajaHallinta.PäivitäTaiLisääPelaaja(pelaaja);
                if (success)
                {
                    if (!PelaajatNimet.Contains(kokonimi))
                    {
                        PelaajatNimet.Add(kokonimi);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Virhe", $"Virhe tallennettaessa pelaajaa: {ex.Message}", "OK");
            }
        }

        private void TilastoTaulukko_Clicked(object sender, EventArgs e)
        {
            var pelaajat = _pelaajaHallinta.LataaPelaajat();
            if (pelaajat != null && pelaajat.Count > 0)
            {
                Navigation.PushAsync(new TilastoTaulukko(pelaajat));
            }
            else
            {
                DisplayAlert("Huomautus", "Pelaajia ei löytynyt.", "OK");
            }
        }
    }
}