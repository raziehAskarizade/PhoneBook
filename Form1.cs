using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PhoneBook
{
    public partial class Form1 : Form
    {
        SqlConnection con = new SqlConnection();
        SqlCommand command = new SqlCommand();
        SqlDataAdapter dataAdapter = new SqlDataAdapter();
        DataSet dataSet = new DataSet();
        CurrencyManager currency;
        int EditPosition;
        Region x = new Region();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            con.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" +Application.StartupPath + @"\PhonebookDatabase.mdf;Integrated Security=True";
            con.Open();
            fillInformation();
        }
        public void fillInformation(string info = "select * from PhoneBookTBL")
        {
            command.CommandText = info;
            command.Connection = con;
            dataAdapter.SelectCommand = command;
            dataSet.Clear();
            dataAdapter.Fill(dataSet, "Table1");
            dataGridView1.DataBindings.Clear();
            dataGridView1.DataBindings.Add("datasource", dataSet, "Table1");
            txtName.DataBindings.Clear();
            txtName.DataBindings.Add("text", dataSet, "Table1.FirstName");
            txtFamily.DataBindings.Clear();
            txtFamily.DataBindings.Add("text", dataSet, "Table1.LastName");
            txtTell.DataBindings.Clear();
            txtTell.DataBindings.Add("text", dataSet, "Table1.PhoneNumber");
            txtAddress.DataBindings.Clear();
            txtAddress.DataBindings.Add("text", dataSet, "Table1.Address");
            picture1.DataBindings.Clear();
            picture1.DataBindings.Add("imagelocation", dataSet, "Table1.PictureURL");
            currency = (CurrencyManager)this.BindingContext[dataSet, "Table1"];

        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            SetPosition(0);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            SetPosition(currency.Position + 1);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            SetPosition(currency.Position - 1);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            SetPosition(currency.Count - 1);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            txtName.ReadOnly = false;
            txtFamily.ReadOnly = false;
            txtTell.ReadOnly = false;
            txtAddress.ReadOnly = false;
            txtName.Text = "";
            txtFamily.Text = "";
            txtTell.Text = "";
            txtAddress.Text = "";
            btnNew.Enabled = false;
            btnSave.Enabled = true;
            btnBrowse.Enabled = true;
            txtName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand sc = new SqlCommand();
                sc.CommandText = "insert into PhoneBookTBL values (@name , @family , @tellphone , @address , @picURL)";
                sc.Parameters.AddWithValue("name", txtName.Text);
                sc.Parameters.AddWithValue("family", txtFamily.Text);
                sc.Parameters.AddWithValue("tellphone", txtTell.Text);
                sc.Parameters.AddWithValue("address", txtAddress.Text);
                sc.Parameters.AddWithValue("picURL",copyPicture(picture1.ImageLocation, txtTell.Text));
                sc.Connection = con;
                sc.ExecuteNonQuery();
                btnSave.Enabled = false;
                btnNew.Enabled = true;
                btnBrowse.Enabled = false;
                txtName.ReadOnly = true;
                txtFamily.ReadOnly = true;
                txtTell.ReadOnly = true;
                txtAddress.ReadOnly = true;
                btnNew.Focus();
                fillInformation();
            }
            catch
            {
                MessageBox.Show($"you chosed {txtTell.Text} before!", "same contact", MessageBoxButtons.OK);
                txtTell.Text = "";
                txtName.Text = "";
                txtFamily.Text = "";
                txtAddress.Text = "";
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            currency.Position = e.RowIndex;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult ansewr;
            ansewr = MessageBox.Show($"Are you sure to delete {txtName.Text} {txtFamily.Text}", "Delete Warning", MessageBoxButtons.YesNo);
            if (ansewr == DialogResult.No)
                return;
            SqlCommand commandDel = new SqlCommand();
            commandDel.CommandText = "delete from PhoneBookTBL where phoneNumber=@phoneNum";
            commandDel.Parameters.AddWithValue("phoneNum", txtTell.Text);
            commandDel.Connection = con;
            commandDel.ExecuteNonQuery();
            fillInformation();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (btnEdit.Text == "Edit")
            {
                txtName.ReadOnly = false;
                txtFamily.ReadOnly = false;
                txtTell.ReadOnly = true;
                txtAddress.ReadOnly = false;
                btnBrowse.Enabled = true;
                btnEdit.Text = "Apply";
                txtName.Focus();
                EditPosition = currency.Position;
            }
            else
            {
                SqlCommand commadEdit = new SqlCommand();
                commadEdit.CommandText = "update PhoneBookTBL set FirstName = @firstN , LastName = @lastN , Address = @address , PictureURL = @picURL where phoneNumber = @phoneNum";
                commadEdit.Parameters.AddWithValue("firstN", txtName.Text);
                commadEdit.Parameters.AddWithValue("lastN", txtFamily.Text);
                commadEdit.Parameters.AddWithValue("address", txtAddress.Text);
                commadEdit.Parameters.AddWithValue("phoneNum", txtTell.Text);
                commadEdit.Parameters.AddWithValue("picURL",copyPicture(picture1.ImageLocation,txtTell.Text));
                commadEdit.Connection = con;
                commadEdit.ExecuteNonQuery();
                fillInformation();
                SetPosition(EditPosition);
                txtName.ReadOnly = true;
                txtFamily.ReadOnly = true;
                txtTell.ReadOnly = true;
                txtAddress.ReadOnly = true;
                btnBrowse.Enabled = false;
                btnEdit.Text = "Edit";
                btnNew.Focus();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string find = "select * from PhoneBookTBL where " + cmbSearch.Text + " like '%" + txtFind.Text + "%'";
                fillInformation(find);
            }
            catch
            {
                MessageBox.Show("Type what you are searching for...", "Empty Entry", MessageBoxButtons.OK);
                fillInformation();
            }
        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            btnSearch_Click(null, null);
        }
        private void SetPosition(int index)
        {
            if (index < 0 || index >= currency.Count)
                return;
            currency.Position = index;
            dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[dataGridView1.CurrentCell.ColumnIndex];
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            SetPosition(dataGridView1.CurrentCell.RowIndex);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "image |*.img| PNG|*.PNG| JPG |*.JPG";
            DialogResult ansewr = openFileDialog1.ShowDialog();
            if (ansewr == DialogResult.Cancel)
                return;
            picture1.ImageLocation = openFileDialog1.FileName;
        }
        private string copyPicture(string sourceFile , string key)
        {
            if (sourceFile == "")
                return "";
            string currentPath;
            string newPath;
            currentPath = Application.StartupPath + @"/images/";
            if (Directory.Exists(currentPath) == false)
                Directory.CreateDirectory(currentPath);
            newPath = currentPath + key + sourceFile.Substring(sourceFile.LastIndexOf("."));
            if (File.Exists(newPath))
                File.Delete(newPath);
            File.Copy(sourceFile, newPath);
            return newPath;

        }

        private void picture1_MouseMove(object sender, MouseEventArgs e)
        {
            x = picture1.Region;
            picture1.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        private void picture1_MouseLeave(object sender, EventArgs e)
        {
            picture1.Region = x;
            picture1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}