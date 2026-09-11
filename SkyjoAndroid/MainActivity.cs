using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using SkyjoWPF.Core; // Fait le lien avec ton MoteurJeu
using System;
using System.Linq;

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
            // Création dynamique des 12 boutons pour chaque joueur
            for (int i = 0; i < 12; i++)
            {
                boutonsAdversaire[i] = CreerBoutonCarte();
                layoutGrilleAdversaire.AddView(boutonsAdversaire[i]);

                boutonsJoueur[i] = CreerBoutonCarte();
                int indexClosure = i; // Important pour les événements
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
            jeu = new MoteurJeu(-1); // Initialise ton moteur
            RafraichirAffichage();
        }

        private void RafraichirAffichage()
        {
            // Mise à jour de la défausse
            if (jeu.Defausse.Count > 0)
            {
                Carte defausse = jeu.Defausse.Peek();
                btnDefausse.Text = defausse.Valeur.ToString();
                btnDefausse.SetBackgroundColor(Color.ParseColor(defausse.CouleurFond));
                btnDefausse.SetTextColor(Color.ParseColor(defausse.CouleurTexte));
            }

            // Mise à jour de la grille joueur (MaGrille dans ton code original)
            for (int i = 0; i < 12; i++)
            {
                Carte c = jeu.GrilleP1[i];
                boutonsJoueur[i].Text = c.Affichage;
                boutonsJoueur[i].SetBackgroundColor(Color.ParseColor(c.CouleurFond));
                boutonsJoueur[i].SetTextColor(Color.ParseColor(c.CouleurTexte));
            }

            // Mise à jour de la grille IA (GrilleAdversaire)
            for (int i = 0; i < 12; i++)
            {
                Carte c = jeu.GrilleP2[i];
                boutonsAdversaire[i].Text = c.Affichage;
                boutonsAdversaire[i].SetBackgroundColor(Color.ParseColor(c.CouleurFond));
                boutonsAdversaire[i].SetTextColor(Color.ParseColor(c.CouleurTexte));
            }
        }

        private void BtnPioche_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Vous avez cliqué sur la pioche", ToastLength.Short).Show();
            // Ici, tu pourras réintégrer ta logique de pioche (vientDeLaPioche, etc.)
        }

        private void BtnDefausse_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, "Vous avez cliqué sur la défausse", ToastLength.Short).Show();
        }

        private void CarteJoueur_Click(int index)
        {
            Carte carteCliquee = jeu.GrilleP1[index];
            if (!carteCliquee.EstVisible)
            {
                carteCliquee.EstVisible = true;
                RafraichirAffichage();
            }
        }
    }
}