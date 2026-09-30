using System.Globalization;

namespace Calculatrice.Pages
{
    public partial class MainPage : ContentPage
    {
        // État de la calculatrice
        private decimal numeroDroite = 0;
        private decimal numeroGauche = 0;
        private string operateur = "";
        private string affichageActuel = "";
        private bool enAtentedonneesNouvelles = false;
        private bool enAtenteDeuximOperation = false;
        private Button boutonOperateurActuel = null;

        // Limite de chiffres et précision
        private const int LIMITE_CHIFFRES = 15;
        private const int PRECISION_DECIMALE = 10;

        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gestion du clic sur un chiffre (0-9)
        /// </summary>
        private void OnNumberClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string chiffre = btn.Text;

            // Limitation du nombre de chiffres à 15
            if (affichageActuel.Length >= LIMITE_CHIFFRES)
                return;

            // Si on est en attente de nouvelles données, réinitialiser l'affichage
            if (enAtentedonneesNouvelles)
            {
                affichageActuel = "";
                enAtentedonneesNouvelles = false;
            }

            // Ajouter le chiffre
            affichageActuel += chiffre;
            numeroDroite = decimal.Parse(affichageActuel, CultureInfo.InvariantCulture);
            MajAffichage();
        }

        /// <summary>
        /// Gestion du clic sur la virgule décimale
        /// </summary>
        private void OnDecimalClicked(object sender, EventArgs e)
        {
            // Ajouter un point seulement s'il n'en existe pas
            if (affichageActuel.Contains("."))
                return;

            // Si champ vide, commencer par "0."
            if (string.IsNullOrEmpty(affichageActuel))
                affichageActuel = "0";

            affichageActuel += ".";
            MajAffichage();
        }

        /// <summary>
        /// Gestion du clic sur un opérateur (+, −, ×, ÷)
        /// </summary>
        private void OnOperatorClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string nouvelOperateur = btn.Text;

            // Si une opération est en attente, la calculer d'abord
            if (!string.IsNullOrEmpty(operateur) && !enAtentedonneesNouvelles)
            {
                CalculerResultat();
                numeroDroite = 0;
            }
            else if (string.IsNullOrEmpty(affichageActuel))
            {
                affichageActuel = "0";
            }

            // Mettre à jour l'état
            numeroGauche = decimal.Parse(affichageActuel, CultureInfo.InvariantCulture);
            operateur = nouvelOperateur;
            enAtentedonneesNouvelles = true;
            enAtenteDeuximOperation = true;
            affichageActuel = "";

            // Mettre en évidence l'opérateur
            MettreEnEvidenceOperateur(btn);
            MajAffichage();
        }

        /// <summary>
        /// Gestion du clic sur le bouton égal
        /// </summary>
        private void OnEqualsClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(operateur) || enAtentedonneesNouvelles)
                return;

            if (!string.IsNullOrEmpty(affichageActuel))
                numeroDroite = decimal.Parse(affichageActuel, CultureInfo.InvariantCulture);

            CalculerResultat();
            operateur = "";
            affichageActuel = ResultLabel.Text;
            enAtentedonneesNouvelles = true;
            enAtenteDeuximOperation = false;

            // Restaurer le style des opérateurs
            ReinitialiserStyleOperateurs();
            MajAffichage();
        }

        /// <summary>
        /// Gestion du clic sur AC (All Clear)
        /// </summary>
        private void OnClearClicked(object sender, EventArgs e)
        {
            numeroDroite = 0;
            numeroGauche = 0;
            operateur = "";
            affichageActuel = "";
            enAtentedonneesNouvelles = false;
            enAtenteDeuximOperation = false;

            ReinitialiserStyleOperateurs();
            ResultLabel.Text = "0";
            OperationLabel.Text = "";
        }

        /// <summary>
        /// Gestion du clic sur le bouton retour (⌫)
        /// </summary>
        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(affichageActuel))
                return;

            // Supprimer le dernier caractère
            affichageActuel = affichageActuel.Substring(0, affichageActuel.Length - 1);

            // Si la chaîne est vide ou seulement un point, afficher 0
            if (string.IsNullOrEmpty(affichageActuel) || affichageActuel == ".")
            {
                numeroDroite = 0;
                affichageActuel = "";
                ResultLabel.Text = "0";
            }
            else
            {
                numeroDroite = decimal.Parse(affichageActuel, CultureInfo.InvariantCulture);
                ResultLabel.Text = FormaterNombre(numeroDroite);
            }

            MajAffichage();
        }

        /// <summary>
        /// Gestion du clic sur ± (changement de signe)
        /// </summary>
        private void OnToggleSignClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(affichageActuel))
                return;

            numeroDroite = decimal.Parse(affichageActuel, CultureInfo.InvariantCulture);
            numeroDroite = -numeroDroite;
            affichageActuel = numeroDroite.ToString(CultureInfo.InvariantCulture);
            MajAffichage();
        }

        /// <summary>
        /// Gestion du clic sur % (pourcentage)
        /// </summary>
        private void OnPercentClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(affichageActuel))
                return;

            numeroDroite = decimal.Parse(affichageActuel, CultureInfo.InvariantCulture);

            // Si une opération est en attente (+ ou −), prendre le % du nombre de gauche
            if (enAtenteDeuximOperation && (operateur == "+" || operateur == "−"))
            {
                numeroDroite = numeroGauche * numeroDroite / 100;
            }
            else
            {
                // Sinon, diviser par 100
                numeroDroite = numeroDroite / 100;
            }

            affichageActuel = numeroDroite.ToString(CultureInfo.InvariantCulture);
            MajAffichage();
        }

        /// <summary>
        /// Calculer le résultat de l'opération en attente
        /// </summary>
        private void CalculerResultat()
        {
            try
            {
                decimal resultat = 0;

                switch (operateur)
                {
                    case "+":
                        resultat = numeroGauche + numeroDroite;
                        break;
                    case "−":
                        resultat = numeroGauche - numeroDroite;
                        break;
                    case "×":
                        resultat = numeroGauche * numeroDroite;
                        break;
                    case "÷":
                        if (numeroDroite == 0)
                        {
                            throw new DivideByZeroException();
                        }
                        resultat = numeroGauche / numeroDroite;
                        break;
                }

                // Arrondir à PRECISION_DECIMALE
                resultat = Math.Round(resultat, PRECISION_DECIMALE);

                ResultLabel.Text = FormaterNombre(resultat);
                OperationLabel.Text = $"{FormaterNombre(numeroGauche)} {operateur} {FormaterNombre(numeroDroite)} =";
                FaireScrolDroite();
            }
            catch (DivideByZeroException)
            {
                ResultLabel.Text = "Division par zéro impossible";
                OperationLabel.Text = "";
                numeroGauche = 0;
                numeroDroite = 0;
                operateur = "";
                affichageActuel = "";
            }
            catch (OverflowException)
            {
                ResultLabel.Text = "Débordement arithmétique";
                OperationLabel.Text = "";
                numeroGauche = 0;
                numeroDroite = 0;
                operateur = "";
                affichageActuel = "";
            }
        }

        /// <summary>
        /// Mise à jour de l'affichage
        /// </summary>
        private void MajAffichage()
        {
            if (string.IsNullOrEmpty(affichageActuel))
            {
                ResultLabel.Text = "0";
            }
            else
            {
                ResultLabel.Text = affichageActuel;
            }

            if (enAtenteDeuximOperation && !string.IsNullOrEmpty(affichageActuel))
            {
                OperationLabel.Text = $"{FormaterNombre(numeroGauche)} {operateur}";
                FaireScrolDroite();
            }
        }

        /// <summary>
        /// Formater un nombre pour l'affichage (sans décimales inutiles)
        /// </summary>
        private string FormaterNombre(decimal nombre)
        {
            // Limiter le nombre de chiffres affichables
            string texte = nombre.ToString(CultureInfo.InvariantCulture);

            // Si c'est un nombre entier, enlever les .0
            if (nombre == Math.Truncate(nombre))
            {
                texte = Math.Truncate(nombre).ToString(CultureInfo.InvariantCulture);
            }

            // Limiter la longueur pour l'affichage
            if (texte.Length > LIMITE_CHIFFRES)
                texte = texte.Substring(0, LIMITE_CHIFFRES);

            return texte;
        }

        /// <summary>
        /// Mettre en évidence l'opérateur actuel
        /// </summary>
        private void MettreEnEvidenceOperateur(Button btn)
        {
            ReinitialiserStyleOperateurs();
            boutonOperateurActuel = btn;
            btn.Style = (Style)Resources["OperatorActiveStyle"];
        }

        /// <summary>
        /// Réinitialiser le style des opérateurs
        /// </summary>
        private void ReinitialiserStyleOperateurs()
        {
            if (boutonOperateurActuel != null)
            {
                boutonOperateurActuel.Style = (Style)Resources["OperatorButtonStyle"];
                boutonOperateurActuel = null;
            }
        }

        /// <summary>
        /// Faire défiler le ScrollView à droite
        /// </summary>
        private void FaireScrolDroite()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100); // Petit délai pour laisser le temps au texte de se mettre à jour
                if (OperationScrollView != null)
                {
                    await OperationScrollView.ScrollToAsync(OperationLabel, ScrollToPosition.End, animated: false);
                }
            });
        }

        /// <summary>
        /// Adaptation de l'orientation (portrait/paysage)
        /// </summary>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width > height)
            {
                // Mode paysage
                ResultLabel.FontSize = 36;
            }
            else
            {
                // Mode portrait
                ResultLabel.FontSize = 56;
            }
        }
    }
}
