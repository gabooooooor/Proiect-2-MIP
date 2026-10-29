using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public class SellForm : Form
    {
        private int productId;
        private string productName;
        private int categoryId;
        private int quantityToSell;
        private int remaining;      // -1 = nu exista, -2 = stoc insuficient
        private string errorMessage;

        private NumericUpDown nudCantitate;
        private Button btnSell;
        private Button btnCancel;

        // constructorul primeste produsul selectat din DataGridView
        public SellForm(ProductView p)
        {
            productId = p.Id;
            productName = p.Denumire;
            categoryId = p.IdCategorie;

            this.Text = "Vânzare produs";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(420, 290);

            AddRow("ID:", p.Id.ToString(), 15);
            AddRow("Denumire:", p.Denumire, 45);
            AddRow("Descriere:", p.Descriere, 75);
            AddRow("Categorie:", p.Categorie, 105);
            AddRow("Data intrării:", p.DataIntrarii.ToString("dd.MM.yyyy"), 135);
            AddRow("Data expirării:", p.DataExpirarii.ToString("dd.MM.yyyy"), 165);
            AddRow("Disponibil:", p.Cantitate.ToString(), 195);

            Label l = new Label();
            l.Text = "Cantitate de vândut:";
            l.Left = 15; l.Top = 227; l.Width = 115;

            nudCantitate = new NumericUpDown();
            nudCantitate.Left = 140; nudCantitate.Top = 225; nudCantitate.Width = 100;
            nudCantitate.Minimum = 1;
            nudCantitate.Maximum = Math.Max(1, p.Cantitate);
            nudCantitate.Value = 1;

            btnSell = new Button();
            btnSell.Text = "Vinde";
            btnSell.Left = 80; btnSell.Top = 255; btnSell.Width = 100;
            btnSell.Click += new EventHandler(btnSell_Click);

            btnCancel = new Button();
            btnCancel.Text = "Anulează";
            btnCancel.Left = 195; btnCancel.Top = 255; btnCancel.Width = 100;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(l);
            this.Controls.Add(nudCantitate);
            this.Controls.Add(btnSell);
            this.Controls.Add(btnCancel);
            this.CancelButton = btnCancel;
        }

        private void AddRow(string text, string value, int top)
        {
            Label l1 = new Label();
            l1.Text = text;
            l1.Left = 15; l1.Top = top; l1.Width = 115;

            Label l2 = new Label();
            l2.Text = value;
            l2.Left = 140; l2.Top = top; l2.Width = 260;

            this.Controls.Add(l1);
            this.Controls.Add(l2);
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            quantityToSell = (int)nudCantitate.Value;
            btnSell.Enabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(SellWorker));
        }

        // thread din Thread Pool
        private void SellWorker(object state)
        {
            errorMessage = null;
            remaining = -1;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    Product p = db.Products.Find(productId);
                    if (p == null)
                    {
                        remaining = -1;
                    }
                    else if (p.Cantitate < quantityToSell)
                    {
                        remaining = -2;
                    }
                    else
                    {
                        p.Cantitate = p.Cantitate - quantityToSell;
                        if (p.Cantitate == 0)
                            db.Products.Remove(p);   // stoc 0 -> stergem produsul

                        // salvam vanzarea in istoric
                        IstoricVanzari h = new IstoricVanzari();
                        h.IdProdus = productId;
                        h.DenumireProdus = productName;
                        h.IdCategorie = categoryId;
                        h.Cantitate = quantityToSell;
                        h.DataVanzarii = DateTime.Now;
                        db.Istoric.Add(h);

                        db.SaveChanges();
                        remaining = p.Cantitate;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(SellDone));
            else
                this.Invoke(new MethodInvoker(SellError));
        }

        // thread GUI
        private void SellDone()
        {
            if (remaining == -1)
            {
                MessageBox.Show("Produsul nu mai există.");
                this.DialogResult = DialogResult.OK;
            }
            else if (remaining == -2)
            {
                MessageBox.Show("Stoc insuficient.");
                btnSell.Enabled = true;
            }
            else if (remaining == 0)
            {
                MessageBox.Show("Vândut. Stocul a ajuns la 0, produsul a fost șters.");
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Vândut. Rămas în stoc: " + remaining);
                this.DialogResult = DialogResult.OK;
            }
        }

        private void SellError()
        {
            btnSell.Enabled = true;
            MessageBox.Show("Eroare: " + errorMessage);
        }
    }
}