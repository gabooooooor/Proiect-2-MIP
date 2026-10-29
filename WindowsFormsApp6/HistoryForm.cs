using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    // clasa pentru afisarea in tabel
    public class HistoryView
    {
        public int Id { get; set; }
        public int IdProdus { get; set; }
        public string Produs { get; set; }
        public string Categorie { get; set; }
        public int Cantitate { get; set; }
        public DateTime Data { get; set; }
    }

    public class HistoryForm : Form
    {
        private DataGridView dgv;
        private Label lblStatus;
        private Button btnClose;

        private List<HistoryView> items;
        private string errorMessage;

        public HistoryForm()
        {
            this.Text = "Istoric vânzări";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(700, 400);

            dgv = new DataGridView();
            dgv.Left = 10; dgv.Top = 10; dgv.Width = 680; dgv.Height = 340;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            lblStatus = new Label();
            lblStatus.Left = 10; lblStatus.Top = 365; lblStatus.Width = 400;
            lblStatus.Text = "Se încarcă...";

            btnClose = new Button();
            btnClose.Text = "Închide";
            btnClose.Left = 590; btnClose.Top = 360; btnClose.Width = 100;
            btnClose.DialogResult = DialogResult.Cancel;

            this.Controls.Add(dgv);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnClose);
            this.CancelButton = btnClose;

            this.Load += new EventHandler(HistoryForm_Load);
        }

        private void HistoryForm_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadWorker));
        }

        // thread din Thread Pool
        private void LoadWorker(object state)
        {
            errorMessage = null;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    List<IstoricVanzari> history = db.Istoric.OrderByDescending(h => h.Id).ToList();
                    List<CategorieProdus> categories = db.Categorii.ToList();

                    items = new List<HistoryView>();
                    foreach (IstoricVanzari h in history)
                    {
                        HistoryView v = new HistoryView();
                        v.Id = h.Id;
                        v.IdProdus = h.IdProdus;
                        v.Produs = h.DenumireProdus;
                        v.Cantitate = h.Cantitate;
                        v.Data = h.DataVanzarii;
                        v.Categorie = "-";
                        foreach (CategorieProdus c in categories)
                        {
                            if (c.Id == h.IdCategorie)
                                v.Categorie = c.Denumire;
                        }
                        items.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(ShowItems));
            else
                this.Invoke(new MethodInvoker(ShowError));
        }

        // thread GUI
        private void ShowItems()
        {
            dgv.DataSource = items;
            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["Id"].Width = 50;
                dgv.Columns["IdProdus"].HeaderText = "ID produs";
                dgv.Columns["Data"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
                dgv.Columns["Data"].Width = 130;
            }
            lblStatus.Text = items.Count + " vânzări";
        }

        private void ShowError()
        {
            lblStatus.Text = "Eroare la încărcare";
            MessageBox.Show("Eroare: " + errorMessage);
        }
    }
}