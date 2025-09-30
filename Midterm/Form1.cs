using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Midterm
{
    public partial class Form1 : Form
    {
        private string connectionString = "server=localhost;database=BENICE;uid=root;pwd=;";
        private int selectedId = -1;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;          // allow form to capture key presses
            this.KeyDown += Form1_KeyDown;   // enable Ctrl + D delete and Ctrl + L clear
        }

        // ✅ Load data from users table into DataGridView
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

        // ✅ Load departments from DB into ComboBox
        private void LoadDepartments()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT dept_name FROM departments ORDER BY dept_name ASC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    cbDept.Items.Clear();
                    while (reader.Read())
                    {
                        cbDept.Items.Add(reader["dept_name"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadDepartments();
            cbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDept.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // ✅ Navigation & Insert when Enter is pressed in textboxes
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
                else if (me == txtStudent) InsertRecord();
            }
        }

        // ✅ Insert a new record
        private void InsertRecord()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age))
            {
                MessageBox.Show("Please enter a valid numeric age.");
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
                        cmd.Parameters.AddWithValue("@lastname", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@firstname", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@middlename", txtMiddleName.Text.Trim());
                        cmd.Parameters.AddWithValue("@gender", cbGender.Text);
                        cmd.Parameters.AddWithValue("@age", age);
                        cmd.Parameters.AddWithValue("@dept", cbDept.Text);
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@student", txtStudent.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Data saved successfully!");
                LoadData();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ✅ Populate fields for editing
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
        }

        // ✅ Update selected record
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Please select a record first!");
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age))
            {
                MessageBox.Show("Please enter a valid numeric age.");
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
                        cmd.Parameters.AddWithValue("@lastname", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@firstname", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@middlename", txtMiddleName.Text.Trim());
                        cmd.Parameters.AddWithValue("@gender", cbGender.Text);
                        cmd.Parameters.AddWithValue("@age", age);
                        cmd.Parameters.AddWithValue("@dept", cbDept.Text);
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@student", txtStudent.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Record updated successfully!");
                LoadData();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // ✅ Live search by student field
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, lastname, firstname, middlename, gender, age, dept,
                                    username, `password`, student
                                    FROM users
                                    WHERE student LIKE @search";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@search", MySqlDbType.VarChar).Value = "%" + txtSearch.Text.Trim() + "%";
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

        // ✅ Capture keyboard shortcuts
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D)
            {
                e.SuppressKeyPress = true; // prevent beep
                DeleteSelectedRecord();
            }
            else if (e.Control && e.KeyCode == Keys.L)
            {
                e.SuppressKeyPress = true; // prevent beep
                ClearFields();
                MessageBox.Show("Fields cleared!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ✅ Delete selected record
        private void DeleteSelectedRecord()
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Please select a record first before deleting!");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this record?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM users WHERE id=@id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Record deleted successfully!");
                    LoadData();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting record: " + ex.Message);
                }
            }
        }

        // ✅ Clear input fields
        private void ClearFields()
        {
            selectedId = -1;
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtAge.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtStudent.Clear();
            cbGender.SelectedIndex = -1;
            cbDept.SelectedIndex = -1;
            txtLastName.Focus();
        }
    }
}