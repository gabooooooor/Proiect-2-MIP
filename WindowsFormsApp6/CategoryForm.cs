using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public class CategoryForm : Form
    {
        private DataGridView dgv;
        private TextBox tbNume;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnClose;

        private List<CategorieProdus> cats;
        private string newName;
        private int deleteId;
        private string infoMessage;
        private string errorMessage;

        public CategoryForm()
        {
            this.Text = "Categorii produse";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(420, 380);

            dgv = new DataGridView();
            dgv.Left = 15; dgv.Top = 15; dgv.Width = 390; dgv.Height = 240;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            Label l1 = new Label();
            l1.Text = "Denumire:";
            l1.Left = 15; l1.Top = 275; l1.Width = 70;

            tbNume = new TextBox();
            tbNume.Left = 90; tbNume.Top = 272; tbNume.Width = 200;
            tbNume.MaxLength = 50;

            btnAdd = new Button();
            btnAdd.Text = "Adaugă";
            btnAdd.Left = 300; btnAdd.Top = 270; btnAdd.Width = 100;
            btnAdd.Click += new EventHandler(btnAdd_Click);

            btnDelete = new Button();
            btnDelete.Text = "Șterge selectata";
            btnDelete.Left = 15; btnDelete.Top = 320; btnDelete.Width = 130;
            btnDelete.Click += new EventHandler(btnDelete_Click);

            btnClose = new Button();
            btnClose.Text = "Închide";
            btnClose.Left = 300; btnClose.Top = 320; btnClose.Width = 100;
            btnClose.DialogResult = DialogResult.Cancel;

            this.Controls.Add(dgv);
            this.Controls.Add(l1);
            this.Controls.Add(tbNume);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnClose);
            this.CancelButton = btnClose;

            this.Load += new EventHandler(CategoryForm_Load);
        }

        private void CategoryForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
        }

        // ---------- incarcare ----------

        private void LoadCategories()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadWorker));
        }

        private void LoadWorker(object state)
        {
            errorMessage = null;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    cats = db.Categorii.OrderBy(c => c.Denumire).ToList();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(ShowCategories));
            else
                this.Invoke(new MethodInvoker(ShowError));
        }

        private void ShowCategories()
        {
            dgv.DataSource = cats;
            if (dgv.Columns.Count > 0)
                dgv.Columns["Id"].Width = 50;
        }

        // ---------- adaugare ----------

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (tbNume.Text.Trim().Length == 0)
            {
                MessageBox.Show("Introduceți denumirea categoriei.");
                return;
            }
            newName = tbNume.Text.Trim();
            infoMessage = null;
            btnAdd.Enabled = false;
            btnDelete.Enabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(AddWorker));
        }

        private void AddWorker(object state)
        {
            errorMessage = null;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    bool exists = db.Categorii.Any(c => c.Denumire == newName);
                    if (exists)
                    {
                        infoMessage = "Categoria există deja.";
                    }
                    else
                    {
                        CategorieProdus c = new CategorieProdus();
                        c.Denumire = newName;
                        db.Categorii.Add(c);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(OperationDone));
            else
                this.Invoke(new MethodInvoker(ShowError));
        }

        // ---------- stergere ----------

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            CategorieProdus c = dgv.CurrentRow.DataBoundItem as CategorieProdus;
            if (c == null) return;

            DialogResult r = MessageBox.Show("Ștergeți categoria " + c.Denumire + "?",
                "Confirmare", MessageBoxButtons.YesNo);
            if (r != DialogResult.Yes) return;

            deleteId = c.Id;
            infoMessage = null;
            btnAdd.Enabled = false;
            btnDelete.Enabled = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(DeleteWorker));
        }

        private void DeleteWorker(object state)
        {
            errorMessage = null;
            try
            {
                using (StoreDbContext db = new StoreDbContext())
                {
                    bool used = db.Products.Any(p => p.IdCategorie == deleteId);
                    if (used)
                    {
                        infoMessage = "Categoria are produse, nu poate fi ștearsă.";
                    }
                    else
                    {
                        CategorieProdus c = db.Categorii.Find(deleteId);
                        if (c != null)
                        {
                            db.Categorii.Remove(c);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ErrorHelper.GetMessage(ex);
            }

            if (errorMessage == null)
                this.Invoke(new MethodInvoker(OperationDone));
            else
                this.Invoke(new MethodInvoker(ShowError));
        }

        // ---------- GUI ----------

        private void OperationDone()
        {
            btnAdd.Enabled = true;
            btnDelete.Enabled = true;
            if (infoMessage != null)
                MessageBox.Show(infoMessage);
            else
                tbNume.Clear();
            LoadCategories();
        }

        private void ShowError()
        {
            btnAdd.Enabled = true;
            btnDelete.Enabled = true;
            MessageBox.Show("Eroare: " + errorMessage);
        }
    }
}