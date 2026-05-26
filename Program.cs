using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace NteRobloxPatcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PatcherForm());
        }
    }

    public class PatcherForm : Form
    {
        // Windows API to draw rounded corners on Windows 11 if supported
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constants for window drag
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        // Core Theme Colors (Tokyo Night / Catppuccin style)
        private static readonly Color ColorBg = Color.FromArgb(26, 27, 38);        // Deep navy-charcoal background
        private static readonly Color ColorTitleBg = Color.FromArgb(22, 22, 30);   // Darker title bar
        private static readonly Color ColorCardBg = Color.FromArgb(36, 40, 59);    // Card background
        private static readonly Color ColorCardBorder = Color.FromArgb(59, 66, 97); // Normal card border
        
        private static readonly Color ColorRoblox = Color.FromArgb(0, 162, 255);    // Roblox Accent Blue
        private static readonly Color ColorNte = Color.FromArgb(187, 154, 247);    // NTE Accent Purple
        private static readonly Color ColorTextPrimary = Color.FromArgb(192, 202, 245); // Warm off-white
        private static readonly Color ColorTextSecondary = Color.FromArgb(108, 117, 159); // Cool gray
        private static readonly Color ColorTextMuted = Color.FromArgb(86, 95, 137);

        // Localization state
        private static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>();
        private static string currentLanguage = "ko";

        // Co-run mode state
        private int originalDriverStart = -1;
        private bool temporaryRobloxPatchApplied = false;

        // State variables
        private int currentDriverStart = -1; // -1: Unknown/Not Installed, 2: NTE, 4: Roblox
        private int selectedMode = 0;        // 0: None, 2: NTE, 4: Roblox

        // Controls
        private Panel panelTitleBar;
        private Label lblTitle;
        private Button btnClose;
        private Button btnMinimize;

        // Dynamic Language UI Controls
        private Label lblLanguageSelector;
        private Label lblLangKo;
        private Label lblLangEn;

        private Panel panelStatus;
        private Label lblStatusTitle;
        private Label lblStatusDesc;
        private Panel panelStatusDot;

        // Card Labels mapped to member variables for dynamic localization
        private Label lblRobloxLogo;
        private Label lblRobloxTitle;
        private Label lblRobloxDesc;
        private Label lblNteLogo;
        private Label lblNteTitle;
        private Label lblNteDesc;

        private Panel cardRoblox;
        private Panel cardNte;
        private CustomCheckbox chkRestart;
        private CustomButton btnApply;

        public PatcherForm()
        {
            // Set Form Properties
            this.Size = new Size(580, 500);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorBg;
            this.Text = "NTE & ROBLOX MULTI PATCHER";

            // Load translations before building components
            LoadTranslations();

            // Apply Windows 11 rounded corners window style if possible
            try
            {
                int attrValue = 2; // DWMW_WINDOW_CORNER_PREFERENCE: DWMWCP_ROUND
                DwmSetWindowAttribute(this.Handle, 33, ref attrValue, sizeof(int));
            }
            catch { }

            InitializeComponents();
            ApplyLanguage(); // Initial translation apply
            ReadRegistryState();
            
            // Apply Roblox Co-run automatic bypass if driver is active (Start=2)
            TryApplyRobloxBypassOnStart();
            
            UpdateUIState();
        }

        private void InitializeComponents()
        {
            // 1. Title Bar Panel
            panelTitleBar = new Panel
            {
                Size = new Size(this.Width, 40),
                Location = new Point(0, 0),
                BackColor = ColorTitleBg
            };
            panelTitleBar.MouseDown += TitleBar_MouseDown;

            lblTitle = new Label
            {
                Text = "🎮 NTE & ROBLOX 플레이 패처",
                Font = new Font("Malgun Gothic", 10F, FontStyle.Bold),
                ForeColor = ColorTextPrimary,
                Location = new Point(15, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            lblTitle.MouseDown += TitleBar_MouseDown;

            btnClose = new Button
            {
                Size = new Size(40, 40),
                Location = new Point(this.Width - 40, 0),
                FlatStyle = FlatStyle.Flat,
                Text = "✕",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ColorTextSecondary,
                BackColor = Color.Transparent
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(243, 139, 168); // Red
            btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(235, 111, 146);
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.White;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = ColorTextSecondary;

            btnMinimize = new Button
            {
                Size = new Size(40, 40),
                Location = new Point(this.Width - 80, 0),
                FlatStyle = FlatStyle.Flat,
                Text = "─",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ColorTextSecondary,
                BackColor = Color.Transparent
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 50, 68);
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.ForeColor = Color.White;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.ForeColor = ColorTextSecondary;

            // Language Selector UI Controls
            lblLanguageSelector = new Label
            {
                Text = "🌐",
                Font = new Font("Segoe UI Emoji", 10F),
                ForeColor = ColorTextSecondary,
                Location = new Point(this.Width - 215, 10),
                Size = new Size(20, 20),
                BackColor = Color.Transparent
            };

            lblLangKo = new Label
            {
                Text = "KO",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                ForeColor = ColorNte,
                Location = new Point(this.Width - 195, 9),
                Size = new Size(30, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblLangKo.Click += (s, e) => {
                if (currentLanguage != "ko")
                {
                    currentLanguage = "ko";
                    ApplyLanguage();
                }
            };

            Label lblDivider = new Label
            {
                Text = "|",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Regular),
                ForeColor = ColorTextMuted,
                Location = new Point(this.Width - 165, 8),
                Size = new Size(10, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblLangEn = new Label
            {
                Text = "EN",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Regular),
                ForeColor = ColorTextSecondary,
                Location = new Point(this.Width - 155, 9),
                Size = new Size(30, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblLangEn.Click += (s, e) => {
                if (currentLanguage != "en")
                {
                    currentLanguage = "en";
                    ApplyLanguage();
                }
            };

            panelTitleBar.Controls.Add(lblTitle);
            panelTitleBar.Controls.Add(lblLanguageSelector);
            panelTitleBar.Controls.Add(lblLangKo);
            panelTitleBar.Controls.Add(lblDivider);
            panelTitleBar.Controls.Add(lblLangEn);
            panelTitleBar.Controls.Add(btnMinimize);
            panelTitleBar.Controls.Add(btnClose);
            this.Controls.Add(panelTitleBar);

            // 2. Status Panel (Current State)
            panelStatus = new Panel
            {
                Size = new Size(530, 70),
                Location = new Point(25, 55),
                BackColor = Color.FromArgb(30, 31, 46)
            };
            panelStatus.Paint += PanelStatus_Paint;

            panelStatusDot = new Panel
            {
                Size = new Size(12, 12),
                Location = new Point(20, 29),
                BackColor = ColorTextMuted
            };
            panelStatusDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Brush b = new SolidBrush(panelStatusDot.BackColor))
                {
                    e.Graphics.FillEllipse(b, 0, 0, panelStatusDot.Width - 1, panelStatusDot.Height - 1);
                }
            };

            lblStatusTitle = new Label
            {
                Text = "현재 모드 감지 중...",
                Font = new Font("Malgun Gothic", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(42, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblStatusDesc = new Label
            {
                Text = "시스템 레지스트리에서 Hunter Anti-Cheat 드라이버 값을 분석하고 있습니다.",
                Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular),
                ForeColor = ColorTextSecondary,
                Location = new Point(42, 38),
                Width = 470,
                Height = 20,
                BackColor = Color.Transparent
            };

            panelStatus.Controls.Add(panelStatusDot);
            panelStatus.Controls.Add(lblStatusTitle);
            panelStatus.Controls.Add(lblStatusDesc);
            this.Controls.Add(panelStatus);

            // 3. Selection Cards
            // 3a. Roblox Card
            cardRoblox = new Panel
            {
                Size = new Size(255, 210),
                Location = new Point(25, 140),
                BackColor = Color.FromArgb(30, 31, 46),
                Cursor = Cursors.Hand
            };
            cardRoblox.Paint += CardRoblox_Paint;
            cardRoblox.Click += CardRoblox_Click;

            lblRobloxLogo = new Label
            {
                Text = "ROBLOX PLAY",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorRoblox,
                Location = new Point(15, 20),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblRobloxLogo.Click += CardRoblox_Click;

            lblRobloxTitle = new Label
            {
                Text = "로블록스 플레이 모드",
                Font = new Font("Malgun Gothic", 11F, FontStyle.Bold),
                ForeColor = ColorTextPrimary,
                Location = new Point(15, 55),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblRobloxTitle.Click += CardRoblox_Click;

            lblRobloxDesc = new Label
            {
                Text = "• Hunter 안티치트 끄기 (Start=4)\n• 로블록스 보안 기능과의 충돌 우려를 완벽히 차단합니다.\n• 쾌적하게 일반 로블록스를 플레이할 수 있습니다.",
                Font = new Font("Malgun Gothic", 9F, FontStyle.Regular),
                ForeColor = ColorTextSecondary,
                Location = new Point(15, 90),
                Width = 225,
                Height = 110,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblRobloxDesc.Click += CardRoblox_Click;

            cardRoblox.Controls.Add(lblRobloxLogo);
            cardRoblox.Controls.Add(lblRobloxTitle);
            cardRoblox.Controls.Add(lblRobloxDesc);
            this.Controls.Add(cardRoblox);

            // 3b. NTE Card
            cardNte = new Panel
            {
                Size = new Size(255, 210),
                Location = new Point(300, 140),
                BackColor = Color.FromArgb(30, 31, 46),
                Cursor = Cursors.Hand
            };
            cardNte.Paint += CardNte_Paint;
            cardNte.Click += CardNte_Click;

            lblNteLogo = new Label
            {
                Text = "NTE PLAY",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorNte,
                Location = new Point(15, 20),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblNteLogo.Click += CardNte_Click;

            lblNteTitle = new Label
            {
                Text = "NTE 플레이 모드",
                Font = new Font("Malgun Gothic", 11F, FontStyle.Bold),
                ForeColor = ColorTextPrimary,
                Location = new Point(15, 55),
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblNteTitle.Click += CardNte_Click;

            lblNteDesc = new Label
            {
                Text = "• Hunter 안티치트 켜기 (Start=2)\n• NTE 접속을 위해 필수적인 보안 드라이버를 활성화합니다.\n• 재시작 후 정상적으로 서버를 이용하실 수 있습니다.",
                Font = new Font("Malgun Gothic", 9F, FontStyle.Regular),
                ForeColor = ColorTextSecondary,
                Location = new Point(15, 90),
                Width = 225,
                Height = 110,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblNteDesc.Click += CardNte_Click;

            cardNte.Controls.Add(lblNteLogo);
            cardNte.Controls.Add(lblNteTitle);
            cardNte.Controls.Add(lblNteDesc);
            this.Controls.Add(cardNte);

            // 4. Action Section
            chkRestart = new CustomCheckbox
            {
                Text = "설정 변경 후 즉시 컴퓨터 재시작하기 (권장)",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Regular),
                ForeColor = ColorTextPrimary,
                Location = new Point(25, 368),
                Size = new Size(530, 24),
                Checked = true
            };
            this.Controls.Add(chkRestart);

            btnApply = new CustomButton
            {
                Text = "모드를 먼저 선택해 주세요",
                Font = new Font("Malgun Gothic", 11F, FontStyle.Bold),
                ForeColor = ColorTextSecondary,
                BackColor = Color.FromArgb(43, 44, 64),
                Size = new Size(530, 50),
                Location = new Point(25, 410),
                Enabled = false
            };
            btnApply.Click += BtnApply_Click;
            this.Controls.Add(btnApply);

            // Add Mouse Hover hook to cards for custom repainting
            HookHoverEvents(cardRoblox);
            HookHoverEvents(cardNte);
        }

        private void HookHoverEvents(Control control)
        {
            control.MouseEnter += (s, e) => { control.Invalidate(); };
            control.MouseLeave += (s, e) => { control.Invalidate(); };
            foreach (Control child in control.Controls)
            {
                child.MouseEnter += (s, e) => { control.Invalidate(); };
                child.MouseLeave += (s, e) => { control.Invalidate(); };
            }
        }

        // Form Border Dragging Implementation
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // Custom Paints
        private void PanelStatus_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panelStatus.Width - 1, panelStatus.Height - 1);
            using (GraphicsPath path = GetRoundedRect(rect, 8))
            {
                using (SolidBrush bgBrush = new SolidBrush(panelStatus.BackColor))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(47, 51, 73), 1))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
        }

        private void CardRoblox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, cardRoblox.Width - 1, cardRoblox.Height - 1);
            bool isHovered = cardRoblox.ClientRectangle.Contains(cardRoblox.PointToClient(Cursor.Position));
            bool isSelected = (selectedMode == 4);

            using (GraphicsPath path = GetRoundedRect(rect, 10))
            {
                Color currentBg = isSelected ? Color.FromArgb(41, 46, 74) : (isHovered ? Color.FromArgb(33, 35, 52) : Color.FromArgb(29, 30, 44));
                using (SolidBrush bgBrush = new SolidBrush(currentBg))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }

                Color currentBorder = isSelected ? ColorRoblox : (isHovered ? Color.FromArgb(86, 95, 137) : ColorCardBorder);
                float borderWidth = isSelected ? 2.5f : 1.0f;
                using (Pen borderPen = new Pen(currentBorder, borderWidth))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }

                // Draw standard selection indicator
                if (isSelected)
                {
                    using (SolidBrush dotBrush = new SolidBrush(ColorRoblox))
                    {
                        e.Graphics.FillEllipse(dotBrush, cardRoblox.Width - 25, 15, 12, 12);
                    }
                }
            }
        }

        private void CardNte_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, cardNte.Width - 1, cardNte.Height - 1);
            bool isHovered = cardNte.ClientRectangle.Contains(cardNte.PointToClient(Cursor.Position));
            bool isSelected = (selectedMode == 2);

            using (GraphicsPath path = GetRoundedRect(rect, 10))
            {
                Color currentBg = isSelected ? Color.FromArgb(41, 46, 74) : (isHovered ? Color.FromArgb(33, 35, 52) : Color.FromArgb(29, 30, 44));
                using (SolidBrush bgBrush = new SolidBrush(currentBg))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }

                Color currentBorder = isSelected ? ColorNte : (isHovered ? Color.FromArgb(86, 95, 137) : ColorCardBorder);
                float borderWidth = isSelected ? 2.5f : 1.0f;
                using (Pen borderPen = new Pen(currentBorder, borderWidth))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }

                // Draw standard selection indicator
                if (isSelected)
                {
                    using (SolidBrush dotBrush = new SolidBrush(ColorNte))
                    {
                        e.Graphics.FillEllipse(dotBrush, cardNte.Width - 25, 15, 12, 12);
                    }
                }
            }
        }

        // Selection Actions
        private void CardRoblox_Click(object sender, EventArgs e)
        {
            selectedMode = 4;
            cardRoblox.Invalidate();
            cardNte.Invalidate();
            UpdateUIState();
        }

        private void CardNte_Click(object sender, EventArgs e)
        {
            selectedMode = 2;
            cardRoblox.Invalidate();
            cardNte.Invalidate();
            UpdateUIState();
        }

        // Read registry setting
        private void ReadRegistryState()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Start");
                        if (val != null)
                        {
                            currentDriverStart = Convert.ToInt32(val);
                        }
                        else
                        {
                            currentDriverStart = -1;
                        }
                    }
                    else
                    {
                        currentDriverStart = -1;
                    }
                }
            }
            catch (Exception)
            {
                currentDriverStart = -2; // Permissions/Access Error
            }
        }

        // Update UI based on loaded states
        private void UpdateUIState()
        {
            // Co-run Mode Indicator takes highest priority
            if (temporaryRobloxPatchApplied)
            {
                panelStatusDot.BackColor = ColorRoblox;
                lblStatusTitle.Text = GetText("roblox_active_status", "임시 로블록스 플레이 모드 활성화됨");
                lblStatusDesc.Text = GetText("roblox_active_desc", "로블록스 플레이를 위해 HtAntiCheatDriver가 임시로 비활성화 및 중지되었습니다. 이 프로그램이 켜져 있는 동안 로블록스를 플레이할 수 있으며, 종료 시 자동 복구됩니다.");
            }
            else if (currentDriverStart == 4)
            {
                panelStatusDot.BackColor = ColorRoblox;
                lblStatusTitle.Text = GetText("status_roblox");
                lblStatusDesc.Text = GetText("status_roblox_desc");
            }
            else if (currentDriverStart == 2)
            {
                panelStatusDot.BackColor = ColorNte;
                lblStatusTitle.Text = GetText("status_nte");
                lblStatusDesc.Text = GetText("status_nte_desc");
            }
            else if (currentDriverStart == -2)
            {
                panelStatusDot.BackColor = Color.FromArgb(243, 139, 168);
                lblStatusTitle.Text = GetText("status_no_permission");
                lblStatusDesc.Text = GetText("status_no_permission_desc");
            }
            else
            {
                panelStatusDot.BackColor = Color.FromArgb(250, 179, 135);
                lblStatusTitle.Text = GetText("status_unknown");
                lblStatusDesc.Text = GetText("status_unknown_desc");
            }
            panelStatusDot.Invalidate();

            // Bottom action button mapping
            if (selectedMode == 4)
            {
                btnApply.Enabled = true;
                btnApply.Text = GetText("btn_apply_roblox") + (chkRestart.Checked ? " & PC " + (currentLanguage == "ko" ? "재시작" : "Restart") : "");
                btnApply.BackColor = ColorRoblox;
                btnApply.ForeColor = Color.Black;
            }
            else if (selectedMode == 2)
            {
                btnApply.Enabled = true;
                btnApply.Text = GetText("btn_apply_nte") + (chkRestart.Checked ? " & PC " + (currentLanguage == "ko" ? "재시작" : "Restart") : "");
                btnApply.BackColor = ColorNte;
                btnApply.ForeColor = Color.Black;
            }
            else
            {
                btnApply.Enabled = false;
                btnApply.Text = GetText("btn_apply_need_select");
                btnApply.BackColor = Color.FromArgb(43, 44, 64);
                btnApply.ForeColor = ColorTextSecondary;
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (selectedMode != 2 && selectedMode != 4) return;

            // Cancel the temporary bypass restore since user is manually overriding the setting
            temporaryRobloxPatchApplied = false;

            string targetModeName = selectedMode == 4 ? "로블록스(Roblox)" : "NTE(Neo Theater)";
            
            try
            {
                // Write Registry Key
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver"))
                {
                    if (key != null)
                    {
                        key.SetValue("Start", selectedMode, RegistryValueKind.DWord);
                    }
                    else
                    {
                        throw new Exception(currentLanguage == "ko" ? "레지스트리 키를 생성 또는 열 수 없습니다." : "Cannot create or open registry key.");
                    }
                }

                // Try to start/stop service based on manual selection
                if (selectedMode == 4)
                {
                    RunCommand("sc.exe", "stop HtAntiCheatDriver");
                }
                else if (selectedMode == 2)
                {
                    RunCommand("sc.exe", "start HtAntiCheatDriver");
                }

                // Check restart choice
                if (chkRestart.Checked)
                {
                    string successMsg = string.Format(GetText("msg_success_reboot"), targetModeName);
                    MessageBox.Show(
                        successMsg, 
                        GetText("msg_success_title"), 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);

                    // Execute computer restart in 5 seconds
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "shutdown.exe",
                        Arguments = "/r /t 5 /f /c \"" + (currentLanguage == "ko" ? "NTE/ROBLOX 모드 전환 완료! 5초 후에 컴퓨터가 재시작됩니다." : "NTE/ROBLOX Mode Switched! PC will restart in 5 seconds.") + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    
                    Application.Exit();
                }
                else
                {
                    string successMsg = string.Format(GetText("msg_success_noreboot"), targetModeName);
                    MessageBox.Show(
                        successMsg, 
                        GetText("msg_success_title"), 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);

                    ReadRegistryState();
                    UpdateUIState();
                }
            }
            catch (Exception ex)
            {
                string failMsg = string.Format(GetText("msg_fail_desc"), ex.Message);
                MessageBox.Show(
                    failMsg,
                    GetText("msg_fail_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Helper to draw clean rounded rectangles
        public static GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        // Handle Checkbox changed to update the apply button label text
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            chkRestart.CheckedChanged += (s, ev) => UpdateUIState();
        }

        // --- Roblox Co-run & Localization Helpers ---

        private void LoadTranslations()
        {
            string langPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang.json");
            string jsonContent = "";
            try
            {
                if (!File.Exists(langPath))
                {
                    File.WriteAllText(langPath, DefaultJsonContent, System.Text.Encoding.UTF8);
                    jsonContent = DefaultJsonContent;
                }
                else
                {
                    jsonContent = File.ReadAllText(langPath, System.Text.Encoding.UTF8);
                }
                
                translations = ParseJson(jsonContent);
                
                // Auto-detect culture
                string sysLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
                if (translations.ContainsKey(sysLang))
                {
                    currentLanguage = sysLang;
                }
                else
                {
                    currentLanguage = "en"; // Default to English for other languages
                }
            }
            catch (Exception)
            {
                translations = ParseJson(DefaultJsonContent);
                currentLanguage = "ko";
            }
        }

        private void ApplyLanguage()
        {
            lblTitle.Text = GetText("title");
            
            lblRobloxLogo.Text = GetText("card_roblox_logo");
            lblRobloxTitle.Text = GetText("card_roblox_title");
            lblRobloxDesc.Text = GetText("card_roblox_desc");
            
            lblNteLogo.Text = GetText("card_nte_logo");
            lblNteTitle.Text = GetText("card_nte_title");
            lblNteDesc.Text = GetText("card_nte_desc");
            
            chkRestart.Text = GetText("chk_restart");
            
            // Dynamic Language Selector highlights
            lblLangKo.ForeColor = (currentLanguage == "ko") ? ColorNte : ColorTextMuted;
            lblLangKo.Font = new Font("Malgun Gothic", 9.5F, (currentLanguage == "ko") ? FontStyle.Bold : FontStyle.Regular);
            
            lblLangEn.ForeColor = (currentLanguage == "en") ? ColorNte : ColorTextMuted;
            lblLangEn.Font = new Font("Malgun Gothic", 9.5F, (currentLanguage == "en") ? FontStyle.Bold : FontStyle.Regular);
            
            UpdateUIState();
        }

        private static string GetText(string key, string defaultValue = "")
        {
            if (translations.ContainsKey(currentLanguage) && translations[currentLanguage].ContainsKey(key))
            {
                return translations[currentLanguage][key];
            }
            if (translations.ContainsKey("en") && translations["en"].ContainsKey(key))
            {
                return translations["en"][key];
            }
            if (translations.ContainsKey("ko") && translations["ko"].ContainsKey(key))
            {
                return translations["ko"][key];
            }
            return defaultValue;
        }

        private static Dictionary<string, Dictionary<string, string>> ParseJson(string json)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            if (string.IsNullOrEmpty(json)) return result;

            try
            {
                int i = 0;
                string currentLang = null;
                
                while (i < json.Length)
                {
                    char c = json[i];
                    if (c == '"')
                    {
                        int start = i + 1;
                        int end = json.IndexOf('"', start);
                        while (end != -1 && json[end - 1] == '\\')
                        {
                            end = json.IndexOf('"', end + 1);
                        }
                        if (end == -1) break;
                        string token = json.Substring(start, end - start);
                        i = end + 1;
                        
                        while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
                        if (i < json.Length && json[i] == ':')
                        {
                            i++;
                            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
                            if (i < json.Length && json[i] == '{')
                            {
                                currentLang = token;
                                result[currentLang] = new Dictionary<string, string>();
                                i++;
                            }
                            else if (i < json.Length && json[i] == '"')
                            {
                                int valStart = i + 1;
                                int valEnd = json.IndexOf('"', valStart);
                                while (valEnd != -1 && json[valEnd - 1] == '\\')
                                {
                                    valEnd = json.IndexOf('"', valEnd + 1);
                                }
                                if (valEnd == -1) break;
                                string val = json.Substring(valStart, valEnd - valStart);
                                
                                val = val.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                                
                                if (currentLang != null)
                                {
                                    result[currentLang][token] = val;
                                }
                                i = valEnd + 1;
                            }
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            catch (Exception)
            {
                // Parser fallback
            }
            return result;
        }

        private void TryApplyRobloxBypassOnStart()
        {
            if (currentDriverStart == 2)
            {
                originalDriverStart = 2;
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver"))
                    {
                        if (key != null)
                        {
                            key.SetValue("Start", 4, RegistryValueKind.DWord);
                            temporaryRobloxPatchApplied = true;
                        }
                    }
                    RunCommand("sc.exe", "stop HtAntiCheatDriver");
                }
                catch (Exception)
                {
                    temporaryRobloxPatchApplied = false;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (temporaryRobloxPatchApplied)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver"))
                    {
                        if (key != null)
                        {
                            key.SetValue("Start", 2, RegistryValueKind.DWord);
                        }
                    }
                    RunCommand("sc.exe", "start HtAntiCheatDriver");
                }
                catch (Exception)
                {
                }
            }
            base.OnFormClosing(e);
        }

        private static void RunCommand(string fileName, string arguments)
        {
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = fileName;
                    p.StartInfo.Arguments = arguments;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit(3000);
                }
            }
            catch { }
        }

        private static readonly string DefaultJsonContent = @" {
  ""ko"": {
    ""title"": ""🎮 NTE & ROBLOX 플레이 패처"",
    ""status_detecting"": ""현재 모드 감지 중..."",
    ""status_desc_detecting"": ""시스템 레지스트리에서 Hunter Anti-Cheat 드라이버 값을 분석하고 있습니다."",
    ""status_roblox"": ""현재 모드: 로블록스 플레이 가능 (Roblox Mode)"",
    ""status_roblox_desc"": ""HtAntiCheatDriver 서비스가 비활성화(Disabled) 되어 로블록스가 정상 작동합니다."",
    ""status_nte"": ""현재 모드: NTE 플레이 가능 (NTE Mode)"",
    ""status_nte_desc"": ""HtAntiCheatDriver 서비스가 활성화(Automatic) 되어 NTE 치트방지가 켜졌습니다."",
    ""status_no_permission"": ""현재 모드: 권한 없음 (접근 거부됨)"",
    ""status_no_permission_desc"": ""레지스트리 읽기 권한이 없습니다. 반드시 관리자 권한으로 실행해 주십시오."",
    ""status_unknown"": ""현재 모드: 드라이버 미설정 또는 알 수 없음"",
    ""status_unknown_desc"": ""HtAntiCheatDriver 서비스가 등록되어 있지 않습니다. 모드 적용 시 생성됩니다."",
    ""card_roblox_logo"": ""ROBLOX PLAY"",
    ""card_roblox_title"": ""로블록스 플레이 모드"",
    ""card_roblox_desc"": ""• Hunter 안티치트 끄기 (Start=4)\n• 로블록스 보안 기능과의 충돌 우려를 완벽히 차단합니다.\n• 쾌적하게 일반 로블록스를 플레이할 수 있습니다."",
    ""card_nte_logo"": ""NTE PLAY"",
    ""card_nte_title"": ""NTE 플레이 모드"",
    ""card_nte_desc"": ""• Hunter 안티치트 켜기 (Start=2)\n• NTE 접속을 위해 필수적인 보안 드라이버를 활성화합니다.\n• 재시작 후 정상적으로 서버를 이용하실 수 있습니다."",
    ""chk_restart"": ""설정 변경 후 즉시 컴퓨터 재시작하기 (권장)"",
    ""btn_apply_select"": ""모드를 먼저 선택해 주세요"",
    ""btn_apply_roblox"": ""로블록스 플레이 모드 설정 적용하기"",
    ""btn_apply_nte"": ""NTE 플레이 모드 설정 적용하기"",
    ""btn_apply_need_select"": ""로블록스나 NTE 모드 중 하나를 선택해 주세요"",
    ""msg_success_title"": ""설정 완료"",
    ""msg_success_reboot"": ""{0} 모드 설정이 성공적으로 저장되었습니다!\n확인을 누르면 컴퓨터가 5초 후에 재시작됩니다."",
    ""msg_success_noreboot"": ""{0} 모드 설정이 적용되었습니다!\n변경 사항을 시스템에 반영하려면 컴퓨터를 직접 다시 시작해 주세요."",
    ""msg_fail_title"": ""적용 실패"",
    ""msg_fail_desc"": ""설정을 적용하는 도중 오류가 발생했습니다.\n관리자 권한이 누락되었거나 시스템 보완 프로그램에 의해 차단되었을 수 있습니다.\n\n오류 원인: {0}"",
    ""roblox_active_status"": ""임시 로블록스 플레이 모드 활성화됨"",
    ""roblox_active_desc"": ""로블록스 플레이를 위해 HtAntiCheatDriver가 임시로 비활성화 및 중지되었습니다.\n이 프로그램이 켜져 있는 동안 로블록스를 플레이할 수 있으며, 종료 시 자동 복구됩니다.""
  },
  ""en"": {
    ""title"": ""🎮 NTE & ROBLOX Play Patcher"",
    ""status_detecting"": ""Detecting current mode..."",
    ""status_desc_detecting"": ""Analyzing Hunter Anti-Cheat driver values in the system registry."",
    ""status_roblox"": ""Current Mode: Roblox Playable (Roblox Mode)"",
    ""status_roblox_desc"": ""HtAntiCheatDriver service is Disabled. Roblox will run normally."",
    ""status_nte"": ""Current Mode: NTE Playable (NTE Mode)"",
    ""status_nte_desc"": ""HtAntiCheatDriver service is Automatic. NTE Anti-Cheat is enabled."",
    ""status_no_permission"": ""Current Mode: No Permission (Access Denied)"",
    ""status_no_permission_desc"": ""No registry read permission. Please run as Administrator."",
    ""status_unknown"": ""Current Mode: Driver Not Configured or Unknown"",
    ""status_unknown_desc"": ""HtAntiCheatDriver service is not registered. It will be created on apply."",
    ""card_roblox_logo"": ""ROBLOX PLAY"",
    ""card_roblox_title"": ""Roblox Play Mode"",
    ""card_roblox_desc"": ""• Disable Hunter Anti-Cheat (Start=4)\n• Completely prevents conflicts with Roblox security features.\n• Enjoy smooth normal Roblox gameplay."",
    ""card_nte_logo"": ""NTE PLAY"",
    ""card_nte_title"": ""NTE Play Mode"",
    ""card_nte_desc"": ""• Enable Hunter Anti-Cheat (Start=2)\n• Activates the security driver required to connect to NTE.\n• Restart required to use the server normally."",
    ""chk_restart"": ""Restart computer immediately after applying changes (Recommended)"",
    ""btn_apply_select"": ""Please select a mode first"",
    ""btn_apply_roblox"": ""Apply Roblox Play Mode settings"",
    ""btn_apply_nte"": ""Apply NTE Play Mode settings"",
    ""btn_apply_need_select"": ""Please select either Roblox or NTE mode"",
    ""msg_success_title"": ""Configuration Complete"",
    ""msg_success_reboot"": ""{0} mode settings saved successfully!\nClick OK to restart the computer in 5 seconds."",
    ""msg_success_noreboot"": ""{0} mode settings applied!\nPlease restart your computer manually to reflect the changes."",
    ""msg_fail_title"": ""Application Failed"",
    ""msg_fail_desc"": ""An error occurred while applying settings.\nAdministrator privileges may be missing or blocked by system security.\n\nError cause: {0}"",
    ""roblox_active_status"": ""Temporary Roblox Play Mode Active"",
    ""roblox_active_desc"": ""HtAntiCheatDriver is temporarily disabled & stopped for Roblox gameplay.\nKeep this window open while playing Roblox. Closing will auto-restore NTE mode.""
  }
}";
        }

    // Modern Flat Design Controls
    public class CustomButton : Button
    {
        public CustomButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = PatcherForm.GetRoundedRect(rect, 8))
            {
                using (SolidBrush bgBrush = new SolidBrush(this.BackColor))
                {
                    pevent.Graphics.FillPath(bgBrush, path);
                }
                
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, rect, this.ForeColor, flags);
            }
        }
    }

    public class CustomCheckbox : Control
    {
        private bool _checked = false;
        public bool Checked 
        { 
            get { return _checked; } 
            set { _checked = value; } 
        }
        public event EventHandler CheckedChanged;

        private static readonly Color ColorActive = Color.FromArgb(0, 162, 255);
        private static readonly Color ColorInactive = Color.FromArgb(59, 66, 97);
        private static readonly Color ColorBoxBg = Color.FromArgb(30, 31, 46);

        public CustomCheckbox()
        {
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
        }

        protected override void OnClick(EventArgs e)
        {
            this.Checked = !this.Checked;
            EventHandler handler = CheckedChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            this.Invalidate();
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Clear Background
            using (SolidBrush bgBrush = new SolidBrush(this.Parent.BackColor))
            {
                e.Graphics.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Draw Box
            Rectangle boxRect = new Rectangle(2, (this.Height - 18) / 2, 18, 18);
            using (GraphicsPath boxPath = PatcherForm.GetRoundedRect(boxRect, 4))
            {
                using (SolidBrush boxBg = new SolidBrush(ColorBoxBg))
                {
                    e.Graphics.FillPath(boxBg, boxPath);
                }

                Color borderCol = Checked ? ColorActive : ColorInactive;
                using (Pen borderPen = new Pen(borderCol, Checked ? 2 : 1))
                {
                    e.Graphics.DrawPath(borderPen, boxPath);
                }
            }

            // Draw Checkmark
            if (Checked)
            {
                using (Pen checkPen = new Pen(Color.White, 2.5f))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    
                    PointF[] checkPoints = new PointF[]
                    {
                        new PointF(boxRect.Left + 4.5f, boxRect.Top + 9.5f),
                        new PointF(boxRect.Left + 8.0f, boxRect.Top + 13.0f),
                        new PointF(boxRect.Left + 13.5f, boxRect.Top + 5.5f)
                    };
                    e.Graphics.DrawLines(checkPen, checkPoints);
                }
            }

            // Draw Text
            Rectangle textRect = new Rectangle(28, 0, this.Width - 28, this.Height);
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, flags);
        }
    }
}
