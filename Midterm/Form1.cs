using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Midterm
{
    public partial class Form1 : Form
    {
        private string connectionString = "server=localhost;database=midterm;uid=root;pwd=;";
        private int selectedId = -1;

        public Form1()
        {
            InitializeComponent();
        }

        private void TextBoxes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                Control me = sender as Control;

                if (me == txtLastName) txtFirstName.Focus();
                else if (me == txtFirstName) txtMiddleName.Focus();
                else if (me == txtMiddleName) cbGender.Focus();
                else if (me == cbGender) txtAge.Focus();
                else if (me == txtAge) cbDept.Focus();
                else if (me == cbDept) txtUsername.Focus();
                else if (me == txtUsername) txtPassword.Focus();
                else if (me == txtPassword) txtStudent.Focus();
                else if (me == txtStudent)
                {
                    if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                        string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                        string.IsNullOrWhiteSpace(txtUsername.Text) ||
                        string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        MessageBox.Show("Please fill in all required fields.");
                        return;
                    }

                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string query = @"INSERT INTO users
                                    (lastname, firstname, middlename, gender, age, dept, username, `password`, student)
                                    VALUES
                                    (@lastname, @firstname, @middlename, @gender, @age, @dept, @username, @password, @student)";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@lastname", txtLastName.Text);
                                cmd.Parameters.AddWithValue("@firstname", txtFirstName.Text);
                                cmd.Parameters.AddWithValue("@middlename", txtMiddleName.Text);
                                cmd.Parameters.AddWithValue("@gender", cbGender.Text);
                                cmd.Parameters.AddWithValue("@age", Convert.ToInt32(txtAge.Text));
                                cmd.Parameters.AddWithValue("@dept", cbDept.Text);
                                cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                                cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                                cmd.Parameters.AddWithValue("@student", txtStudent.Text);

                                cmd.ExecuteNonQuery();
                                LoadData();
                            }
                        }

                        MessageBox.Show("Data saved successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }

                    txtLastName.Clear();
                    txtFirstName.Clear();
                    txtMiddleName.Clear();
                    txtAge.Clear();
                    txtUsername.Clear();
                    txtPassword.Clear();
                    txtStudent.Clear();
                    txtLastName.Focus();
                }
            }
        }

        private void LoadData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT id, lastname, firstname, middlename, gender, age, dept, username, `password`, student FROM users";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            cbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDept.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["id"].Value != null)
                    selectedId = Convert.ToInt32(row.Cells["id"].Value);

                txtLastName.Text = row.Cells["lastname"].Value.ToString();
                txtFirstName.Text = row.Cells["firstname"].Value.ToString();
                txtMiddleName.Text = row.Cells["middlename"].Value.ToString();
                txtAge.Text = row.Cells["age"].Value.ToString();
                txtUsername.Text = row.Cells["username"].Value.ToString();
                txtPassword.Text = row.Cells["password"].Value.ToString();
                txtStudent.Text = row.Cells["student"].Value.ToString();

                SetComboBoxValue(cbGender, row.Cells["gender"].Value.ToString().Trim());
                SetComboBoxValue(cbDept, row.Cells["dept"].Value.ToString().Trim());
            }
        }

        private void SetComboBoxValue(ComboBox cb, string value)
        {
            int index = cb.FindStringExact(value);
            cb.SelectedIndex = index >= 0 ? index : -1;
            cb.Refresh();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Please select a record first!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE users SET
                            lastname=@lastname, firstname=@firstname, middlename=@middlename,
                            gender=@gender, age=@age, dept=@dept,
                            username=@username, `password`=@password, student=@student
                            WHERE id=@id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@lastname", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@firstname", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@middlename", txtMiddleName.Text);
                        cmd.Parameters.AddWithValue("@gender", cbGender.Text);
                        cmd.Parameters.AddWithValue("@age", Convert.ToInt32(txtAge.Text));
                        cmd.Parameters.AddWithValue("@dept", cbDept.Text);
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@student", txtStudent.Text);
                        cmd.Parameters.AddWithValue("@id", selectedId);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Record updated successfully!");
                LoadData();
                txtLastName.Clear();
                txtFirstName.Clear();
                txtMiddleName.Clear();
                txtAge.Clear();
                txtUsername.Clear();
                txtPassword.Clear();
                txtStudent.Clear();
                txtLastName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void SearchData(string searchTerm)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT id, lastname, firstname, middlename, gender, age, dept, username, `password`, student
                             FROM users
                             WHERE CONCAT(student) LIKE @search";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@search", MySqlDbType.VarChar).Value = "%" + searchTerm + "%";

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching data: " + ex.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            SearchData(txtSearch.Text.Trim());
        }

        private void txtStudent_TextChanged(object sender, EventArgs e)
        {
        }
    }
}