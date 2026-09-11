using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using SkyjoWPF.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkyjoAndroid
{
    [Activity(Label = "Skyjo", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity
    {
        MoteurJeu jeu;
        GridLayout layoutGrilleJoueur;
        GridLayout layoutGrilleAdversaire;
        Button btnPioche;
        Button btnDefausse;

        Button[] boutonsJoueur = new Button[12];
        Button[] boutonsAdversaire = new Button[12];

        // Variables d'état du jeu
        bool tourDuJoueur = true;
        Carte carteEnMain = null;
        bool vientDeLaPioche = false;
        bool doitRetournerCarte = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            layoutGrilleJoueur = FindViewById<GridLayout>(Resource.Id.grilleJoueur);
            layoutGrilleAdversaire = FindViewById<GridLayout>(Resource.Id.grilleAdversaire);
            btnPioche = FindViewById<Button>(Resource.Id.btnPioche);
            btnDefausse = FindViewById<Button>(Resource.Id.btnDefausse);

            btnPioche.Click += BtnPioche_Click;
            btnDefausse.Click += BtnDefausse_Click;

            InitialiserGrillesVisuelles();
            LancerPartieSolo();
        }

        private void InitialiserGrillesVisuelles()
        {
            for (int i = 0; i < 12; i++)
            {
                boutonsAdversaire[i] = CreerBoutonCarte();
                layoutGrilleAdversaire.AddView(boutonsAdversaire[i]);

                boutonsJoueur[i] = CreerBoutonCarte();
                int indexClosure = i;
                boutonsJoueur[i].Click += (s, e) => CarteJoueur_Click(indexClosure);
                layoutGrilleJoueur.AddView(boutonsJoueur[i]);
            }
        }

        private Button CreerBoutonCarte()
        {
            var btn = new Button(this)
            {
                LayoutParameters = new GridLayout.LayoutParams
                {
                    Width = 150,
                    Height = 220,
                    RightMargin = 5,
                    BottomMargin = 5
                },
                TextSize = 20
            };
            btn.SetTypeface(null, TypefaceStyle.Bold);
            return btn;
        }

        private void LancerPartieSolo()
        {
            jeu = new MoteurJeu(-1);
            tourDuJoueur = true;
            carteEnMain = null;
            vientDeLaPioche = false;
            doitRetournerCarte = false;
            RafraichirAffichage();
        }

        // ==========================================
        // GESTION SÉCURISÉE DES COULEURS (CORRECTION BUG)
        // ==========================================
        private Color ParseColorSafe(string colorStr)
        {
            if (string.Equals(colorStr, "Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return Color.Transparent;
            }
            return Color.ParseColor(colorStr);
        }

        private void RafraichirAffichage()
        {
            if (jeu.Defausse.Count > 0)
            {
                Carte defausse = jeu.Defausse.Peek();
                btnDefausse.Text = defausse.Valeur.ToString();
                btnDefausse.SetBackgroundColor(ParseColorSafe(defausse.CouleurFond));
                btnDefausse.SetTextColor(ParseColorSafe(defausse.CouleurTexte));
            }
            else
            {
                btnDefausse.Text = "?";
                btnDefausse.SetBackgroundColor(Color.ParseColor("#ecf0f1"));
                btnDefausse.SetTextColor(Color.ParseColor("#2c3e50"));
            }

            for (int i = 0; i < 12; i++)
            {
                Carte c = jeu.GrilleP1[i];
                boutonsJoueur[i].Text = c.Affichage;
                boutonsJoueur[i].SetBackgroundColor(ParseColorSafe(c.CouleurFond));
                boutonsJoueur[i].SetTextColor(ParseColorSafe(c.CouleurTexte));
            }

            for (int i = 0; i < 12; i++)
            {
                Carte c = jeu.GrilleP2[i];
                boutonsAdversaire[i].Text = c.Affichage;
                boutonsAdversaire[i].SetBackgroundColor(ParseColorSafe(c.CouleurFond));
                boutonsAdversaire[i].SetTextColor(ParseColorSafe(c.CouleurTexte));
            }

            btnPioche.Text = carteEnMain != null && vientDeLaPioche ? $"En main:\n{carteEnMain.Valeur}" : "Pioche\n(Dos)";
            btnPioche.SetBackgroundColor(ParseColorSafe(carteEnMain != null && vientDeLaPioche ? carteEnMain.CouleurFond : "#34495e"));
        }

        // ==========================================
        // LOGIQUE DES ACTIONS DU JOUEUR
        // ==========================================

        private void BtnPioche_Click(object sender, EventArgs e)
        {
            if (!tourDuJoueur) return;
            if (doitRetournerCarte)
            {
                AfficherMessage("Attention", "Vous devez d'abord retourner une de vos cartes face cachée !");
                return;
            }

            if (carteEnMain == null)
            {
                carteEnMain = jeu.TirerCarte();
                carteEnMain.EstVisible = true;
                vientDeLaPioche = true;
                RafraichirAffichage();
                AfficherMessage("Pioche", $"Vous avez pioché : {carteEnMain.Valeur}\n\nAppuyez sur une de vos cartes pour la remplacer, ou sur la défausse pour la jeter.");
            }
        }

        private void BtnDefausse_Click(object sender, EventArgs e)
        {
            if (!tourDuJoueur) return;

            if (carteEnMain == null && jeu.Defausse.Count > 0)
            {
                if (doitRetournerCarte) return;
                carteEnMain = jeu.Defausse.Pop();
                vientDeLaPioche = false;
                RafraichirAffichage();
                Toast.MakeText(this, $"Vous avez pris le {carteEnMain.Valeur}. Sélectionnez une carte à remplacer.", ToastLength.Long).Show();
            }
            else if (carteEnMain != null && vientDeLaPioche)
            {
                jeu.Defausse.Push(carteEnMain);
                carteEnMain = null;
                vientDeLaPioche = false;
                doitRetournerCarte = true;
                RafraichirAffichage();
                AfficherMessage("Règle Skyjo", "Carte jetée. Vous DEVEZ maintenant cliquer sur une de vos cartes FACE CACHÉE pour la révéler.");
            }
        }

        private async void CarteJoueur_Click(int index)
        {
            if (!tourDuJoueur) return;
            Carte carteCliquee = jeu.GrilleP1[index];

            if (carteCliquee != null && !carteCliquee.EstVide)
            {
                if (doitRetournerCarte)
                {
                    if (carteCliquee.EstVisible)
                    {
                        Toast.MakeText(this, "Vous devez impérativement retourner une carte FACE CACHÉE.", ToastLength.Short).Show();
                        return;
                    }
                    carteCliquee.EstVisible = true;
                    doitRetournerCarte = false;
                }
                else if (carteEnMain != null)
                {
                    carteCliquee.EstVisible = true;
                    jeu.Defausse.Push(carteCliquee);
                    jeu.GrilleP1[index] = carteEnMain;
                    carteEnMain = null;
                    vientDeLaPioche = false;
                }
                else if (!carteCliquee.EstVisible)
                {
                    carteCliquee.EstVisible = true;
                }
                else return;

                VerifierColonnes(jeu.GrilleP1);
                RafraichirAffichage();

                if (PartieEstFinie(jeu.GrilleP1))
                {
                    TerminerPartie("Vous avez");
                    return;
                }

                await JouerTourIA();
            }
        }

        // ==========================================
        // INTELLIGENCE ARTIFICIELLE & ANIMATIONS
        // ==========================================

        private async Task AnimerActionIA(Button boutonCible)
        {
            boutonCible.Animate().ScaleX(1.15f).ScaleY(1.15f).SetDuration(200).Start();
            await Task.Delay(200);
            boutonCible.Animate().ScaleX(1f).ScaleY(1f).SetDuration(200).Start();
            await Task.Delay(150);
        }

        private async Task JouerTourIA()
        {
            tourDuJoueur = false;
            await Task.Delay(800);

            Carte carteChoisie = null;
            bool prendDefausse = false;
            bool jeterEtRetourner = false;

            int indexPireVisible = -1;
            int pireValeur = -99;
            for (int i = 0; i < 12; i++)
            {
                if (!jeu.GrilleP2[i].EstVide && jeu.GrilleP2[i].EstVisible && jeu.GrilleP2[i].Valeur > pireValeur)
                {
                    pireValeur = jeu.GrilleP2[i].Valeur;
                    indexPireVisible = i;
                }
            }

            if (jeu.Defausse.Count > 0 && (jeu.Defausse.Peek().Valeur <= 2 || (pireValeur > 4 && jeu.Defausse.Peek().Valeur < pireValeur - 2)))
            {
                await AnimerActionIA(btnDefausse);
                carteChoisie = jeu.Defausse.Pop();
                prendDefausse = true;
            }
            else
            {
                await AnimerActionIA(btnPioche);
                carteChoisie = jeu.TirerCarte();
                carteChoisie.EstVisible = true;
            }

            await Task.Delay(600);

            int indexCible = -1;
            if (!prendDefausse && carteChoisie.Valeur > 6)
            {
                jeterEtRetourner = true;
                indexCible = Array.FindIndex(jeu.GrilleP2, c => !c.EstVisible && !c.EstVide);
                if (indexCible == -1) jeterEtRetourner = false;
            }

            if (!jeterEtRetourner)
            {
                if (indexPireVisible != -1 && carteChoisie.Valeur <= pireValeur)
                    indexCible = indexPireVisible;

                if (indexCible == -1)
                {
                    indexCible = Array.FindIndex(jeu.GrilleP2, c => !c.EstVisible && !c.EstVide);
                    if (indexCible == -1) indexCible = Array.FindIndex(jeu.GrilleP2, c => !c.EstVide);
                }
            }

            if (jeterEtRetourner)
            {
                await AnimerActionIA(btnDefausse);
                jeu.Defausse.Push(carteChoisie);
                await Task.Delay(300);
                await AnimerActionIA(boutonsAdversaire[indexCible]);
                jeu.GrilleP2[indexCible].EstVisible = true;
            }
            else
            {
                await AnimerActionIA(boutonsAdversaire[indexCible]);
                Carte ancienne = jeu.GrilleP2[indexCible];
                ancienne.EstVisible = true;
                jeu.Defausse.Push(ancienne);
                jeu.GrilleP2[indexCible] = carteChoisie;
            }

            VerifierColonnes(jeu.GrilleP2);
            RafraichirAffichage();

            if (PartieEstFinie(jeu.GrilleP2))
            {
                TerminerPartie("L'IA a");
                return;
            }

            tourDuJoueur = true;
        }

        private void VerifierColonnes(Carte[] grille)
        {
            for (int c = 0; c < 4; c++)
            {
                Carte c1 = grille[c]; Carte c2 = grille[c + 4]; Carte c3 = grille[c + 8];
                if (!c1.EstVide && !c2.EstVide && !c3.EstVide &&
                    c1.EstVisible && c2.EstVisible && c3.EstVisible &&
                    c1.Valeur == c2.Valeur && c2.Valeur == c3.Valeur)
                {
                    Carte defaussePropre = new Carte(c1.Valeur);
                    defaussePropre.EstVisible = true;
                    jeu.Defausse.Push(defaussePropre);
                    c1.EstVide = true; c2.EstVide = true; c3.EstVide = true;
                }
            }
        }

        private bool PartieEstFinie(Carte[] grille)
        {
            return grille.All(c => c.EstVide || c.EstVisible);
        }

        private void TerminerPartie(string declencheur)
        {
            tourDuJoueur = false;
            foreach (var c in jeu.GrilleP1) c.EstVisible = true;
            foreach (var c in jeu.GrilleP2) c.EstVisible = true;

            VerifierColonnes(jeu.GrilleP1);
            VerifierColonnes(jeu.GrilleP2);
            RafraichirAffichage();

            int scoreJoueur = jeu.GrilleP1.Where(c => !c.EstVide).Sum(c => c.Valeur);
            int scoreIA = jeu.GrilleP2.Where(c => !c.EstVide).Sum(c => c.Valeur);

            string resultat = $"{declencheur} retourné toutes ses cartes !\n\nVotre score : {scoreJoueur}\nScore IA : {scoreIA}\n\n";
            if (scoreJoueur < scoreIA) resultat += "🏆 VOUS AVEZ GAGNÉ !";
            else if (scoreJoueur > scoreIA) resultat += "💀 VOUS AVEZ PERDU !";
            else resultat += "🤝 ÉGALITÉ !";

            new AlertDialog.Builder(this)
                .SetTitle("Fin de la partie")
                .SetMessage(resultat)
                .SetCancelable(false)
                .SetPositiveButton("Rejouer", (s, ev) => LancerPartieSolo())
                .Show();
        }

        private void AfficherMessage(string titre, string message)
        {
            new AlertDialog.Builder(this)
                .SetTitle(titre)
                .SetMessage(message)
                .SetPositiveButton("OK", (s, e) => { })
                .Show();
        }
    }
}