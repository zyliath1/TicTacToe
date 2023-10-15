using System.Collections.ObjectModel;
using System.Text.Json;

namespace final_work
{
    public partial class MainPage : ContentPage
    {
        // Tämä on pelaajien hallinta, joka lataa ja tallentaa pelaajat tiedostoon (JSON)
        private readonly PelaajaHallinta _pelaajaHallinta;

        // Tämä on pelaajien nimet, jotka näytetään pudotusvalikossa (Picker)
        public ObservableCollection<string> PelaajatNimet { get; set; } = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();

            // Lataa pelaajat
            var tallennus = new TiedostoPelaajanTallennus();
            _pelaajaHallinta = new PelaajaHallinta(tallennus);

            // Lataa vastustajan valinta JSON-tiedostosta
            this.BindingContext = this;
            PelaajatNimet.Clear();

            // Lataa pelaajat ja lisää ne listaan
            foreach (var pelaaja in _pelaajaHallinta.LataaPelaajat())
            {
                if (pelaaja.Etunimi != "Tietokone")
                {
                    PelaajatNimet.Add($"{pelaaja.Etunimi} {pelaaja.Sukunimi}");
                }
            }
        }

        // Tämä on staattinen tietokonepelaaja, jotta sitä ei koskaan muuteta
        public static readonly Pelaaja TietokonePelaaja = new Pelaaja
        {
            Etunimi = "Tietokone",
            PelaajaId = Guid.Empty
        };

        // Käsittelee aloita peli -napin painalluksen ja aloittaa pelin valitun pelaajan ja vastustajan kanssa (tietokone tai toinen pelaaja)
        private async void AloitaPeliClicked(object sender, EventArgs e)
        {
            if (PelaajaPicker.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse pelaaja ennen pelin aloittamista.", "OK");
                return;
            }

            // Tarkistaa onko pelaaja jo listalla
            var valitunPelaajanNimi = PelaajaPicker.SelectedItem.ToString();
            var nimenOsat = JaaNimi(valitunPelaajanNimi);
            if (nimenOsat == null)
            {
                await DisplayAlert("Virhe", "Virheellinen nimi.", "OK");
                return;
            }
            var valittuPelaaja = _pelaajaHallinta.HaePelaaja(nimenOsat[0], nimenOsat[1]);

            Pelaaja vastustaja = await ValitseVastustaja(valittuPelaaja);
            if (vastustaja == null) // Tässä oletetaan, että ValitseVastustaja palauttaa null, jos on ongelma.
            {
                // Virheilmoitus käsitellään ValitseVastustaja metodissa.
                return;
            }

            await Navigation.PushAsync(new PeliSivu(valittuPelaaja, vastustaja));
        }

        // Tämä palauttaa pelaajan etu- ja sukunimen osiin pilkulla eroteltuna
        private string[] JaaNimi(string pelaajanNimi)
        {
            var nimenOsat = pelaajanNimi.Split(' ');
            return nimenOsat.Length == 2 ? nimenOsat : null;
        }

        // Valitsee vastustajan pelaajan valinnan mukaan (tietokone tai toinen pelaaja)
        private async Task<Pelaaja> ValitseVastustaja(Pelaaja valittuPelaaja)
        {
            if (TietokoneRadio.IsChecked)
            {
                return TietokonePelaaja;
            }

            if (VastustajaPicker.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Valitse vastustaja.", "OK");
                return null;
            }

            var valitunPelaajanNimi = VastustajaPicker.SelectedItem.ToString();
            var vastustajanNimenOsat = JaaNimi(valitunPelaajanNimi);
            if (vastustajanNimenOsat == null)
            {
                await DisplayAlert("Virhe", "Virheellinen nimi.", "OK");
                return null;
            }

            var vastustaja = _pelaajaHallinta.HaePelaaja(vastustajanNimenOsat[0], vastustajanNimenOsat[1]);

            if (valittuPelaaja.PelaajaId == vastustaja.PelaajaId)
            {
                await DisplayAlert("Virhe", "Et voi pelata itseäsi vastaan.", "OK");
                return null;
            }
            return vastustaja;
        }

        // Käsittelee vastustajan valinnan muutoksen
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
            Dictionary<string, string> data;

            try
            {
                // Yritä lukea tiedostoa suoraan ja käsittele poikkeus, jos tiedostoa ei ole olemassa
                var content = File.ReadAllText(jsonFilePath);
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            }
            catch (FileNotFoundException)
            {
                // Jos tiedostoa ei ole olemassa, luo uusi Dictionary
                data = new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                // Muut mahdolliset poikkeukset, esim. tiedoston lukuongelmat
                Console.WriteLine($"Virhe tiedoston luvussa: {ex.Message}");
                return;
            }

            // Aseta tai päivitä vastustaja
            data["vastustaja"] = vastustaja;

            // Tallenna päivitetty JSON-tiedosto siististi muotoiltuna
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var jsonContent = JsonSerializer.Serialize(data, options);
            File.WriteAllText(jsonFilePath, jsonContent);
        }

        // Tämä käsittelee pelaajan valinnan muutoksen ja tallentaa valinnan JSON-tiedostoon
        private async void TallennaTiedotClicked(object sender, EventArgs e)
        {
            string etunimi = EtunimiEntry.Text;
            string sukunimi = SukunimiEntry.Text;

            if (string.IsNullOrWhiteSpace(etunimi) || string.IsNullOrWhiteSpace(sukunimi))
            {
                await DisplayAlert("Virhe", "Syötä etu- ja sukunimi!", "OK");
                return;
            }

            if (!int.TryParse(SyntymäVuosiEntry.Text, out int syntymävuosi))
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

            // Luo uusi pelaaja ja tallenna se tiedostoon
            var uusiPelaaja = new Pelaaja
            {
                Etunimi = etunimi,
                Sukunimi = sukunimi,
                Syntymävuosi = syntymävuosi,
                Voitot = 0,
                Tappiot = 0,
                Tasapelit = 0,
                PelienYhteiskesto = TimeSpan.Zero
            };

            TallennaPelaaja(uusiPelaaja);
            EtunimiEntry.Text = "";
            SukunimiEntry.Text = "";
            SyntymäVuosiEntry.Text = "";
            await DisplayAlert("Vahvistus", "Tiedot tallennettu onnistuneesti!", "OK");
        }

        // Tallentaa pelaajan tiedot tiedostoon ja lisää pelaajan nimen pudotusvalikkoon
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

        // Näyttää tilastotaulukon pelaajista ja heidän tilastoistaan (voitot, tappiot, tasapelit, pelien yhteiskesto)
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