using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lab04_01.Models;

namespace Lab04_01
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<Faculty> faculties;

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (StudentContextDB context = new StudentContextDB())
                {
                    List<Student> students = context.Students.ToList();
                    faculties = context.Faculties.ToList();  // Lưu faculties vào biến toàn cục
                    FillFacultyCombobox(faculties);
                    BindGrid(students);
                }
                // Cập nhật kiểu font cho các ô dữ liệu
                dgvStudent.DefaultCellStyle.Font = new Font("Arial", 9);

                // Cập nhật kiểu font cho tiêu đề cột (Header)
                dgvStudent.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);

                // Cập nhật chiều cao dòng
                dgvStudent.RowTemplate.Height = 22;

                // Cập nhật chế độ tự động điều chỉnh cột
                dgvStudent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvStudent.SelectionChanged += dgvStudent_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }


        private void FillFacultyCombobox(List<Faculty> faculties)
        {
            cmbFaculty.DataSource = faculties;
            cmbFaculty.DisplayMember = "FacultyName";
            cmbFaculty.ValueMember = "FacultyID";
        }

        private void BindGrid(List<Student> students)
        {
            dgvStudent.Rows.Clear();

            foreach (var student in students)
            {
                // Tra cứu tên khoa từ danh sách faculties đã được lưu
                string facultyName = faculties.FirstOrDefault(f => f.FacultyID == student.FacultyID)?.FacultyName ?? "Chưa gán khoa";
                dgvStudent.Rows.Add(student.StudentID, student.FullName, facultyName, student.AverageScore);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string id = txtID.Text.Trim();
                if (string.IsNullOrEmpty(id) || id.Length != 10)
                {
                    MessageBox.Show("Mã sinh viên phải có đúng 10 ký tự.");
                    return;
                }

                string name = txtName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Họ tên không được để trống.");
                    return;
                }

                int facultyID = (int)cmbFaculty.SelectedValue;
                if (facultyID == 0)
                {
                    MessageBox.Show("Vui lòng chọn một khoa.");
                    return;
                }

                if (!double.TryParse(txtAvgScore.Text, out double avgScore))
                {
                    MessageBox.Show("Điểm trung bình không hợp lệ.");
                    return;
                }

                using (StudentContextDB context = new StudentContextDB())
                {
                    context.Students.Add(new Student
                    {
                        StudentID = id,
                        FullName = name,
                        FacultyID = facultyID,
                        AverageScore = avgScore
                    });
                    context.SaveChanges();
                }

                RefreshGrid();
                ClearInput();
                MessageBox.Show("Thêm sinh viên thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sinh viên: " + ex.Message);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvStudent.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một sinh viên.");
                    return;
                }

                string id = txtID.Text.Trim();
                string name = txtName.Text.Trim();
                if (!double.TryParse(txtAvgScore.Text, out double avgScore))
                {
                    MessageBox.Show("Điểm trung bình không hợp lệ.");
                    return;
                }

                int facultyID = (int)cmbFaculty.SelectedValue;

                using (StudentContextDB context = new StudentContextDB())
                {
                    var student = context.Students.Find(id);
                    if (student != null)
                    {
                        student.FullName = name;
                        student.AverageScore = avgScore;
                        student.FacultyID = facultyID;
                        context.SaveChanges();
                    }
                }

                RefreshGrid();
                MessageBox.Show("Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvStudent.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một sinh viên để xóa.");
                    return;
                }

                var confirm = MessageBox.Show("Bạn chắc chắn muốn xóa sinh viên đã chọn?", "Xác nhận xóa", MessageBoxButtons.YesNo);
                if (confirm == DialogResult.Yes)
                {
                    using (StudentContextDB context = new StudentContextDB())
                    {
                        foreach (DataGridViewRow row in dgvStudent.SelectedRows)
                        {
                            string id = row.Cells[0].Value.ToString();
                            var student = context.Students.Find(id);
                            if (student != null)
                                context.Students.Remove(student);
                        }
                        context.SaveChanges();
                    }
                    RefreshGrid();
                    MessageBox.Show("Xóa thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message);
            }
        }

        private void dgvStudent_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudent.SelectedRows.Count > 0)
            {
                var row = dgvStudent.SelectedRows[0];
                txtID.Text = row.Cells[0].Value.ToString();
                txtName.Text = row.Cells[1].Value.ToString();
                txtAvgScore.Text = row.Cells[3].Value.ToString();

                string facultyName = row.Cells[2].Value.ToString();
                cmbFaculty.SelectedValue = (cmbFaculty.DataSource as List<Faculty>)?.FirstOrDefault(f => f.FacultyName == facultyName)?.FacultyID;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void RefreshGrid()
        {
            using (StudentContextDB context = new StudentContextDB())
            {
                List<Student> students = context.Students.ToList();
                BindGrid(students);
            }
        }
            
        private void ClearInput()
        {
            txtID.Clear();
            txtName.Clear();
            txtAvgScore.Clear();
            cmbFaculty.SelectedIndex = -1;
        }
    }
}
