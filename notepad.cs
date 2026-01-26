using System;
using System.IO;
using System.Windows.Forms;

namespace notepadsharp
{
    public class Notepad : Form
    {
        private TextBox textBox;
        private MenuStrip menuStrip;
        private string currentFile = null;
        private bool isModified = false;
        private float currentZoom = 1.0f;

        public Notepad()
        {

            // Load icon from embedded resource
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "notepadsharp.sharp.ico"; 
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        this.Icon = new System.Drawing.Icon(stream);
                    }
                }
            }
            catch
            {
                // Icon not found, use default
            }

            // Window setup
            this.Text = "Untitled - notepad#";
            this.Width = 600;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Text box
            textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Courier New", 10),
                ScrollBars = ScrollBars.Both,
                AcceptsTab = true,
                WordWrap = true
            };
            textBox.TextChanged += (s, e) => {
                isModified = true;
                UpdateTitle();
            };
            this.Controls.Add(textBox);

            // Menu bar
            CreateMenus();

            // Handle window close
            this.FormClosing += OnFormClosing;
        }

        private void CreateMenus()
        {
            menuStrip = new MenuStrip();

            // File menu
            var fileMenu = new ToolStripMenuItem("&File");
            
            var newItem = new ToolStripMenuItem("&New", null, OnNew);
            newItem.ShortcutKeys = Keys.Control | Keys.N;
            fileMenu.DropDownItems.Add(newItem);
            
            var openItem = new ToolStripMenuItem("&Open...", null, OnOpen);
            openItem.ShortcutKeys = Keys.Control | Keys.O;
            fileMenu.DropDownItems.Add(openItem);
            
            var saveItem = new ToolStripMenuItem("&Save", null, OnSave);
            saveItem.ShortcutKeys = Keys.Control | Keys.S;
            fileMenu.DropDownItems.Add(saveItem);
            
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Save &As...", null, OnSaveAs));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("&Print...", null, OnPrint));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (s, e) => this.Close()));

            // Edit menu
            var editMenu = new ToolStripMenuItem("&Edit");
            
            var undoItem = new ToolStripMenuItem("&Undo", null, (s, e) => textBox.Undo());
            undoItem.ShortcutKeys = Keys.Control | Keys.Z;
            editMenu.DropDownItems.Add(undoItem);
            
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            
            var cutItem = new ToolStripMenuItem("Cu&t", null, (s, e) => textBox.Cut());
            cutItem.ShortcutKeys = Keys.Control | Keys.X;
            editMenu.DropDownItems.Add(cutItem);
            
            var copyItem = new ToolStripMenuItem("&Copy", null, (s, e) => textBox.Copy());
            copyItem.ShortcutKeys = Keys.Control | Keys.C;
            editMenu.DropDownItems.Add(copyItem);
            
            var pasteItem = new ToolStripMenuItem("&Paste", null, (s, e) => textBox.Paste());
            pasteItem.ShortcutKeys = Keys.Control | Keys.V;
            editMenu.DropDownItems.Add(pasteItem);
            
            var deleteItem = new ToolStripMenuItem("De&lete", null, (s, e) => {
                int start = textBox.SelectionStart;
                int length = textBox.SelectionLength;
                if (length > 0) {
                    textBox.Text = textBox.Text.Remove(start, length);
                    textBox.SelectionStart = start;
                }
            });
            deleteItem.ShortcutKeys = Keys.Delete;
            editMenu.DropDownItems.Add(deleteItem);
            
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            
            var selectAllItem = new ToolStripMenuItem("Select &All", null, (s, e) => textBox.SelectAll());
            selectAllItem.ShortcutKeys = Keys.Control | Keys.A;
            editMenu.DropDownItems.Add(selectAllItem);
            
            var timeDateItem = new ToolStripMenuItem("Time/&Date", null, (s, e) => {
                int pos = textBox.SelectionStart;
                // change the date format if you live outside the US
                textBox.Text = textBox.Text.Insert(pos, DateTime.Now.ToString("h:mm tt M/d/yyyy"));
                textBox.SelectionStart = pos + DateTime.Now.ToString("h:mm tt M/d/yyyy").Length;
            });
            timeDateItem.ShortcutKeys = Keys.F5;
            editMenu.DropDownItems.Add(timeDateItem);
            
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add(new ToolStripMenuItem("&Font...", null, OnFont));

            // Search menu
            var searchMenu = new ToolStripMenuItem("&Search");
            var findItem = new ToolStripMenuItem("&Find...", null, OnFind);
            findItem.ShortcutKeys = Keys.Control | Keys.F;
            searchMenu.DropDownItems.Add(findItem);

            // View menu
            var viewMenu = new ToolStripMenuItem("&View");

            var zoomInItem = new ToolStripMenuItem("Zoom &In", null, OnZoomIn);
            zoomInItem.ShortcutKeys = Keys.Control | Keys.Oemplus; // Ctrl + 
            viewMenu.DropDownItems.Add(zoomInItem);

            var zoomOutItem = new ToolStripMenuItem("Zoom &Out", null, OnZoomOut);
            zoomOutItem.ShortcutKeys = Keys.Control | Keys.OemMinus; // Ctrl - 
            viewMenu.DropDownItems.Add(zoomOutItem);

            var zoomResetItem = new ToolStripMenuItem("&Reset Zoom", null, OnZoomReset);
            zoomResetItem.ShortcutKeys = Keys.Control | Keys.D0; // Ctrl 0
            viewMenu.DropDownItems.Add(zoomResetItem);

            // About menu
            var aboutMenu = new ToolStripMenuItem("&About");
            aboutMenu.DropDownItems.Add(new ToolStripMenuItem("&About notepad#", null, OnAbout));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(searchMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(aboutMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void UpdateTitle()
        {
            string filename = currentFile != null ? Path.GetFileName(currentFile) : "Untitled";
            string modified = isModified ? "*" : "";
            this.Text = $"{modified}{filename} - notepad#";
        }

        private bool CheckSaveChanges()
        {
            if (!isModified) return true;

            string filename = currentFile != null ? Path.GetFileName(currentFile) : "Untitled";
            DialogResult result = MessageBox.Show(
                $"Do you want to save changes to {filename}?",
                "notepad#",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                OnSave(null, null);
                return !isModified; // If still modified, save was cancelled
            }
            else if (result == DialogResult.No)
            {
                return true;
            }
            else
            {
                return false; // Cancel
            }
        }

        private void OnNew(object sender, EventArgs e)
        {
            if (!CheckSaveChanges()) return;

            textBox.Clear();
            currentFile = null;
            isModified = false;
            UpdateTitle();
        }

        private void OnOpen(object sender, EventArgs e)
        {
            if (!CheckSaveChanges()) return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*";
                dialog.Title = "Open";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        textBox.Text = File.ReadAllText(dialog.FileName);
                        currentFile = dialog.FileName;
                        isModified = false;
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cannot open file:\n{ex.Message}", "notepad#", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (currentFile == null)
            {
                OnSaveAs(sender, e);
            }
            else
            {
                try
                {
                    File.WriteAllText(currentFile, textBox.Text);
                    isModified = false;
                    UpdateTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot save file:\n{ex.Message}", "Notepad",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnSaveAs(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*";
                dialog.Title = "Save As";
                dialog.FileName = currentFile != null ? Path.GetFileName(currentFile) : "Untitled.txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, textBox.Text);
                        currentFile = dialog.FileName;
                        isModified = false;
                        UpdateTitle();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cannot save file:\n{ex.Message}", "notepad#",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private Form activeFindDialog = null; // Add this as a class field at the top

private void OnFind(object sender, EventArgs e)
{
    // If dialog already open, just focus it
    if (activeFindDialog != null && !activeFindDialog.IsDisposed)
    {
        activeFindDialog.Focus();
        return;
    }

    // Create Find dialog
    Form findDialog = new Form
    {
        Text = "Find",
        Width = 400,
        Height = 180,
        FormBorderStyle = FormBorderStyle.FixedDialog,
        StartPosition = FormStartPosition.Manual,
        MaximizeBox = false,
        MinimizeBox = false,
        ShowInTaskbar = false,
        Owner = this  // Make it owned by main window
    };

    // Position it in top-right corner of main window
    findDialog.Location = new System.Drawing.Point(
        this.Location.X + this.Width - findDialog.Width - 20,
        this.Location.Y + 50
    );

    Label label = new Label
    {
        Text = "Find what:",
        Left = 10,
        Top = 15,
        Width = 70
    };

    TextBox searchBox = new TextBox
    {
        Left = 85,
        Top = 12,
        Width = 280
    };

    CheckBox caseSensitiveBox = new CheckBox
    {
        Text = "Match case",
        Left = 85,
        Top = 45,
        Width = 100
    };

    Label counterLabel = new Label
    {
        Text = "",
        Left = 85,
        Top = 70,
        Width = 280,
        ForeColor = System.Drawing.Color.Gray
    };

    Button findNextButton = new Button
    {
        Text = "Find Next",
        Left = 200,
        Top = 110,
        Width = 80
    };

    Button closeButton = new Button
    {
        Text = "Close",
        Left = 290,
        Top = 110,
        Width = 80
    };

    closeButton.Click += (s, ev) => findDialog.Close();

    // Keep track of search state
    int lastSearchPos = 0;
    int currentMatchIndex = 0;

    // Helper to count total matches
    int CountMatches(string text, string search, StringComparison comparison)
    {
        int count = 0;
        int pos = 0;
        while ((pos = text.IndexOf(search, pos, comparison)) != -1)
        {
            count++;
            pos += search.Length;
        }
        return count;
    }

    findNextButton.Click += (s, ev) =>
    {
        string search = searchBox.Text;
        if (string.IsNullOrEmpty(search))
        {
            counterLabel.Text = "";
            return;
        }

        StringComparison comparison = caseSensitiveBox.Checked 
            ? StringComparison.Ordinal 
            : StringComparison.OrdinalIgnoreCase;

        // Count total matches
        int totalMatches = CountMatches(textBox.Text, search, comparison);

        if (totalMatches == 0)
        {
            MessageBox.Show($"Cannot find \"{search}\"", "notepad#",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            counterLabel.Text = "No matches found";
            return;
        }

        // Search from last position
        int index = textBox.Text.IndexOf(search, lastSearchPos, comparison);

        // If not found, wrap around to beginning
        if (index < 0 && lastSearchPos > 0)
        {
            index = textBox.Text.IndexOf(search, 0, comparison);
            currentMatchIndex = 0;
        }

        if (index >= 0)
        {
            // Calculate which match number this is
            currentMatchIndex++;
            if (currentMatchIndex > totalMatches) currentMatchIndex = 1;

            // Select and highlight the text
            textBox.Select(index, search.Length);
            textBox.ScrollToCaret();
            textBox.Focus();  // This now works because dialog is modeless

            // Update counter
            counterLabel.Text = $"Match {currentMatchIndex} of {totalMatches}";

            lastSearchPos = index + search.Length;
        }
    };

    // Reset search when text changes
    searchBox.TextChanged += (s, ev) =>
    {
        lastSearchPos = 0;
        currentMatchIndex = 0;
        counterLabel.Text = "";
    };

    caseSensitiveBox.CheckedChanged += (s, ev) =>
    {
        lastSearchPos = 0;
        currentMatchIndex = 0;
        counterLabel.Text = "";
    };

    // Enter key = Find Next
    searchBox.KeyDown += (s, ev) =>
    {
        if (ev.KeyCode == Keys.Enter)
        {
            findNextButton.PerformClick();
            ev.Handled = true;
            ev.SuppressKeyPress = true;
        }
    };

    findDialog.FormClosed += (s, ev) => activeFindDialog = null;

    findDialog.Controls.Add(label);
    findDialog.Controls.Add(searchBox);
    findDialog.Controls.Add(caseSensitiveBox);
    findDialog.Controls.Add(counterLabel);
    findDialog.Controls.Add(findNextButton);
    findDialog.Controls.Add(closeButton);

    activeFindDialog = findDialog;
    searchBox.Focus();
    findDialog.Show();  // Changed from ShowDialog() to Show() - this makes it modeless!
}

        private void OnPrint(object sender, EventArgs e)
        {
            try
            {
                System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.PrintPage += (s, ev) =>
                {
                    ev.Graphics.DrawString(textBox.Text, textBox.Font, System.Drawing.Brushes.Black, ev.MarginBounds);
                };

                System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
                printDialog.Document = printDoc;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot print:\n{ex.Message}", "notepad#",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnFont(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = textBox.Font;
            fontDialog.ShowColor = false;

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Font = fontDialog.Font;
            }
        }

        private void OnZoomIn(object sender, EventArgs e)
        {
            currentZoom += 0.1f;
            if (currentZoom > 5.0f) currentZoom = 5.0f;
            ApplyZoom();
        }

        private void OnZoomOut(object sender, EventArgs e)
        {
            currentZoom -= 0.1f;
            if (currentZoom < 0.1f) currentZoom = 0.1f;
            ApplyZoom();
        }

        private void OnZoomReset(object sender, EventArgs e)
        {
            currentZoom = 1.0f;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            float baseFontSize = 10f;
            textBox.Font = new System.Drawing.Font(textBox.Font.FontFamily, baseFontSize * currentZoom, textBox.Font.Style);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckSaveChanges())
            {
                e.Cancel = true;
            }
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "notepad#\n" +
                "Version 1.0\n\n" +
                "A barebones, no AI text editor.\n\n" +
                "Created by adelon",
                "about",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Notepad());
        }
    }
}
