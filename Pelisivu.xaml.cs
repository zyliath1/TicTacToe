using System.Diagnostics;
using System.Text.Json;

namespace final_work
{
    public partial class PeliSivu : ContentPage
    {
        private Pelaaja _pelaaja1;
        private Pelaaja _pelaaja2;
        private Pelaaja _edellinenVoittaja;
        private string[,] _pelilauta = new string[3, 3];
        private Pelaaja _vuorossa;
        private Stopwatch _peliajanotto = new Stopwatch();
        private Random _rand = new Random();
        private TilastoTaulukko tilastoTaulukkoInstance;

        public PeliSivu(Pelaaja pelaaja1, Pelaaja pelaaja2)
        {
            InitializeComponent();

            _pelaaja1 = pelaaja1;
            _pelaaja2 = pelaaja2;

            LataaPelaajatTiedostosta();

            // Aseta random pelaaja aloittavaksi pelaajaksi
            _vuorossa = _rand.Next(2) == 0 ? _pelaaja1 : _pelaaja2;

            AlustaPeli();

            // Luo tilastotaulukko ja lisää pelaajat siihen
            tilastoTaulukkoInstance = new TilastoTaulukko(new List<Pelaaja> { _pelaaja1, _pelaaja2 });

            if (OnkoTietokoneenVuoro())
            {
                TietokoneenSiirto();
            }
            _peliajanotto.Start();
        }

        // Lataa pelaajat tiedostosta, jos tiedosto on olemassa
        private void LataaPelaajatTiedostosta()
        {
            string tiedostoPolku = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "pelaajat.json");

            if (File.Exists(tiedostoPolku))
            {
                var sisältö = File.ReadAllText(tiedostoPolku);
                List<Pelaaja> pelaajatilastot = JsonSerializer.Deserialize<List<Pelaaja>>(sisältö);

                _pelaaja1 = pelaajatilastot.FirstOrDefault(p => p.Etunimi == _pelaaja1.Etunimi && p.Sukunimi == _pelaaja1.Sukunimi) ?? _pelaaja1;
                _pelaaja2 = pelaajatilastot.FirstOrDefault(p => p.Etunimi == _pelaaja2.Etunimi && p.Sukunimi == _pelaaja2.Sukunimi) ?? _pelaaja2;
            }
        }

        // Alustaa pelilaudan tyhjäksi ja asettaa vuorossa olevan pelaajan UI:hin näkyviin
        private void AlustaPeli()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _pelilauta[i, j] = "";
                }
            }

            UpdateUI();
        }

        // Päivittää vuorossa olevan pelaajan UI:hin näkyviin
        private void UpdateUI()
        {
            VasenVuoroLabel.Text = _vuorossa == _pelaaja1
                ? $"Pelaajan {_pelaaja1.Etunimi} {_pelaaja1.Sukunimi} vuoro"
                : "";

            OikeaVuoroLabel.Text = _vuorossa == _pelaaja2
                ? $"Pelaajan {_pelaaja2.Etunimi} {_pelaaja2.Sukunimi} vuoro"
                : "";
        }

        // Kutsutaan kun ruutua klikataan ja merkitsee ruudun vuorossa olevan pelaajan merkillä (X tai O)
        public void RuutuClicked(object sender, EventArgs e)
        {
            if (sender is Button klikattuNappi && !string.IsNullOrEmpty(klikattuNappi.ClassId) && _vuorossa != null && _vuorossa.Etunimi != "Tietokone")
            {
                var koordinaatit = klikattuNappi.ClassId.Split(',');
                int x = int.Parse(koordinaatit[0]);
                int y = int.Parse(koordinaatit[1]);

                // Lisää tämä ehto estämään uudelleenmerkitseminen
                if (string.IsNullOrEmpty(_pelilauta[x, y]))
                {
                    _pelilauta[x, y] = _vuorossa == _pelaaja1 ? "X" : "O";
                    klikattuNappi.Text = _pelilauta[x, y];

                    PäivitäPelinTila();

                    if (_vuorossa == _pelaaja1 && _pelaaja2.Etunimi == "Tietokone" || _vuorossa == _pelaaja2 && _pelaaja1.Etunimi == "Tietokone")
                        TietokoneenSiirto();
                }
            }
        }

        // Tarkista onko peli päättynyt ja päivittää pelitilanteen sen mukaan (voitto, tasapeli, jatkuu)
        private void PäivitäPelinTila()
        {
            if (OnkoVoittaja())
            {
                _peliajanotto.Stop();
                var kulunutAika = _peliajanotto.Elapsed;
                _pelaaja1.LisääPeliaika(kulunutAika);
                _pelaaja2.LisääPeliaika(kulunutAika);

                string voittoViesti = _vuorossa.Etunimi == "Tietokone"
                    ? "Tietokone voitti!"
                    : $"Pelaaja {_vuorossa.Etunimi} {_vuorossa.Sukunimi} voitti!";

                DisplayAlert("Onnittelut!", voittoViesti, "OK");
                _vuorossa.Voitot++;

                tilastoTaulukkoInstance.PäivitäPelaajaTilastot(_vuorossa);
                if (_vuorossa == _pelaaja1) tilastoTaulukkoInstance.PäivitäPelaajaTilastot(_pelaaja2);
                else tilastoTaulukkoInstance.PäivitäPelaajaTilastot(_pelaaja1);

                if (_vuorossa == _pelaaja1) _pelaaja2.Tappiot++;
                else _pelaaja1.Tappiot++;

                TallennaPelinTulos($"{_vuorossa.Etunimi} {_vuorossa.Sukunimi} voitti");
                _edellinenVoittaja = _vuorossa;
            }
            else if (OnkoTasapeli())
            {
                _peliajanotto.Stop();
                var kulunutAika = _peliajanotto.Elapsed;
                _pelaaja1.LisääPeliaika(kulunutAika);
                _pelaaja2.LisääPeliaika(kulunutAika);

                string TasapeliViesti = $"Peli päättyi tasapeliin pelaajien {_pelaaja1.Etunimi} ja {_pelaaja2.Etunimi} välillä!";
                DisplayAlert("Tasapeli!", TasapeliViesti, "OK");
                _pelaaja1.Tasapelit++;
                _pelaaja2.Tasapelit++;

                tilastoTaulukkoInstance.PäivitäPelaajaTilastot(_vuorossa);
                if (_vuorossa == _pelaaja1) tilastoTaulukkoInstance.PäivitäPelaajaTilastot(_pelaaja2);
                else tilastoTaulukkoInstance.PäivitäPelaajaTilastot(_pelaaja1);

                TallennaPelinTulos("Peli päättyi tasapeliin");
                _edellinenVoittaja = null;
            }
            else
            {
                _vuorossa = _vuorossa == _pelaaja1 ? _pelaaja2 : _pelaaja1;
                UpdateUI();

                if (_vuorossa.Etunimi == "Tietokone")
                {
                    TietokoneenSiirto();
                }
            }
        }

        // Tallentaa pelin tuloksen tiedostoon pelaajien tilastoihin (voitto, tappio, tasapeli ja pelien yhteiskesto)
        private void TallennaPelinTulos(string viesti)
        {
            string tiedostoPolku = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "pelaajat.json");

            List<Pelaaja> pelaajatilastot = new List<Pelaaja>();
            if (File.Exists(tiedostoPolku))
            {
                var sisältö = File.ReadAllText(tiedostoPolku);
                pelaajatilastot = JsonSerializer.Deserialize<List<Pelaaja>>(sisältö);
            }

            // Tarkista onko pelaaja1 jo listassa
            var pelaaja1Index = pelaajatilastot.FindIndex(p => p.Etunimi == _pelaaja1.Etunimi && p.Sukunimi == _pelaaja1.Sukunimi);
            if (pelaaja1Index == -1)
            {
                pelaajatilastot.Add(_pelaaja1);  // Lisää pelaaja1 suoraan listaan, koska se ei ollut siellä aikaisemmin
            }
            else
            {
                pelaajatilastot[pelaaja1Index].Voitot = _pelaaja1.Voitot;
                pelaajatilastot[pelaaja1Index].Tappiot = _pelaaja1.Tappiot;
                pelaajatilastot[pelaaja1Index].Tasapelit = _pelaaja1.Tasapelit;
                pelaajatilastot[pelaaja1Index].PelienYhteiskesto = _pelaaja1.PelienYhteiskesto;
            }

            var pelaaja2Index = pelaajatilastot.FindIndex(p => p.Etunimi == _pelaaja2.Etunimi && p.Sukunimi == _pelaaja2.Sukunimi);
            if (pelaaja2Index == -1)
            {
                pelaajatilastot.Add(_pelaaja2);  // Lisää pelaaja2 suoraan listaan, koska se ei ollut siellä aikaisemmin
            }
            else
            {
                pelaajatilastot[pelaaja2Index].Voitot = _pelaaja2.Voitot;
                pelaajatilastot[pelaaja2Index].Tappiot = _pelaaja2.Tappiot;
                pelaajatilastot[pelaaja2Index].Tasapelit = _pelaaja2.Tasapelit;
                pelaajatilastot[pelaaja2Index].PelienYhteiskesto = _pelaaja2.PelienYhteiskesto;
            }

            var päivitettySisältö = JsonSerializer.Serialize(pelaajatilastot);
            File.WriteAllText(tiedostoPolku, päivitettySisältö);
        }

        private async void TietokoneenSiirto()
        {
            // Odottaa 0.5 - 2 sekuntia ennen siirtoa
            await Task.Delay(_rand.Next(500, 2000));

            int paikkaX, paikkaY;

            // 1. Tarkistaa, voiko tietokone voittaa seuraavalla siirrollaan
            if (EtsiVoittoTaiEstoSiirto(out paikkaX, out paikkaY, _vuorossa))
            {
                // Siirto löydetty
            }
            // 2. Tarkistaa, voiko vastustaja voittaa seuraavalla siirrollaan
            else if (_vuorossa == _pelaaja1 && EtsiVoittoTaiEstoSiirto(out paikkaX, out paikkaY, _pelaaja2) ||
                     _vuorossa == _pelaaja2 && EtsiVoittoTaiEstoSiirto(out paikkaX, out paikkaY, _pelaaja1))
            {
                // Siirto löydetty
            }
            // 3. Jos keskellä oleva ruutu on vapaa, valitsee sen.
            else if (string.IsNullOrEmpty(_pelilauta[1, 1]))
            {
                paikkaX = 1;
                paikkaY = 1;
            }
            // 4. Valitse kulmista vapaa ruutu
            else if (EtsiVapaaKulma(out paikkaX, out paikkaY))
            {
                // Siirto löydetty
            }
            // 5. Valitse mikä tahansa vapaa ruutu
            else
            {
                do
                {
                    paikkaX = _rand.Next(0, 3);
                    paikkaY = _rand.Next(0, 3);
                } while (!string.IsNullOrEmpty(_pelilauta[paikkaX, paikkaY]));
            }

            var button = FindButtonByCoordinates(paikkaX, paikkaY);
            _pelilauta[paikkaX, paikkaY] = _vuorossa == _pelaaja1 ? "X" : "O";
            button.Text = _pelilauta[paikkaX, paikkaY];

            PäivitäPelinTila();
        }

        // Tietokone tarkistaa voiko pelaaja voittaa seuraavalla siirrollaan tai estää vastustajan voiton
        private bool EtsiVoittoTaiEstoSiirto(out int x, out int y, Pelaaja pelaaja)
        {
            string merkki = pelaaja == _pelaaja1 ? "X" : "O";
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(_pelilauta[i, j]))
                    {
                        _pelilauta[i, j] = merkki;
                        if (OnkoVoittaja())
                        {
                            _pelilauta[i, j] = "";
                            x = i;
                            y = j;
                            return true;
                        }
                        _pelilauta[i, j] = "";
                    }
                }
            }
            x = -1;
            y = -1;
            return false;
        }

        // Tietokone valitsee kulmista vapaan ruudun
        private bool EtsiVapaaKulma(out int x, out int y)
        {
            int[,] kulmat = { { 0, 0 }, { 0, 2 }, { 2, 0 }, { 2, 2 } };

            for (int i = 0; i < kulmat.GetLength(0); i++)
            {
                int kulmaX = kulmat[i, 0];
                int kulmaY = kulmat[i, 1];

                if (string.IsNullOrEmpty(_pelilauta[kulmaX, kulmaY]))
                {
                    x = kulmaX;
                    y = kulmaY;
                    return true;
                }
            }

            x = -1;
            y = -1;
            return false;
        }

        private void UusiPeli(object sender, EventArgs e)
        {
            foreach (Button btn in PelilautaGrid.Children)
            {
                btn.Text = "";
            }

            AlustaPeli();

            _peliajanotto.Reset();
            _peliajanotto.Start();

            if (_edellinenVoittaja != null)
            {
                _vuorossa = _edellinenVoittaja;
            }
            else
            {
                // Valitse random pelaaja aloittamaan jos edellinen peli oli tasapeli
                _vuorossa = _rand.Next(2) == 0 ? _pelaaja1 : _pelaaja2;
            }

            UpdateUI();

            if (OnkoTietokoneenVuoro())
            {
                TietokoneenSiirto();
            }
        }

        // Etsi ruudun pelilaudalta koordinaattien perusteella (esim. Ruutu01)
        private Button FindButtonByCoordinates(int x, int y)
        {
            return (Button)this.FindByName($"Ruutu{x}{y}");
        }

        private bool OnkoVoittaja()
        {
            // Tarkista rivit
            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrEmpty(_pelilauta[i, 0]) &&
                    _pelilauta[i, 0] == _pelilauta[i, 1] && _pelilauta[i, 1] == _pelilauta[i, 2])
                {
                    return true;
                }
            }

            // Tarkista sarakkeet
            for (int j = 0; j < 3; j++)
            {
                if (!string.IsNullOrEmpty(_pelilauta[0, j]) &&
                    _pelilauta[0, j] == _pelilauta[1, j] && _pelilauta[1, j] == _pelilauta[2, j])
                {
                    return true;
                }
            }

            // Tarkista diagonaalit
            if (!string.IsNullOrEmpty(_pelilauta[0, 0]) &&
                _pelilauta[0, 0] == _pelilauta[1, 1] && _pelilauta[1, 1] == _pelilauta[2, 2])
            {
                return true;
            }
            if (!string.IsNullOrEmpty(_pelilauta[0, 2]) &&
                _pelilauta[0, 2] == _pelilauta[1, 1] && _pelilauta[1, 1] == _pelilauta[2, 0])
            {
                return true;
            }

            return false;
        }

        private bool OnkoTasapeli()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(_pelilauta[i, j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool OnkoTietokoneenVuoro()
        {
            return _vuorossa.PelaajaId == Guid.Empty;
        }
    }
}