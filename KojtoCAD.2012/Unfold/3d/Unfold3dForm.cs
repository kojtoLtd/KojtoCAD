using System;
using System.Windows.Forms;
#if !bcad
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Application = Bricscad.ApplicationServices.Application;
#endif

namespace KojtoCAD.Unfold._3d
{
  public partial class Unfold3dForm : Form
  {
    public double m_S;
    public double m_R;
    public double m_kF;
    public int pos;

    public Unfold3dForm()
    {
      InitializeComponent();
      radioButtonAirBending.Checked = true;
    }

    public Unfold3dForm(Double S , Double R , Double kF , int Pos)
    {
      InitializeComponent();
      if (Pos == 2)
      {
        kF = 2 * (R + S) - (Math.PI / 2.0) * (R + kF * S);
      }
      m_S = S;
      m_R = R;
      m_kF = kF;
      pos = Pos;
      textBox_S.Text = m_S.ToString();
      textBox_R.Text = m_R.ToString();
      textBox_KFactor.Text = m_kF.ToString();
      switch (pos)
      {
        case 0:
        radioButtonAirBending.Checked = true;
        labelKFactor.Text = "K-Factor";
        break;
        case 1:
        radioButtonBA.Checked = true;
        labelKFactor.Text = "Bend Allowence";
        break;
        case 2:
        radioButtonBD.Checked = true;
        labelKFactor.Text = "Bend Deduction";
        break;
      }
    }

    private void button_OK_Click(object sender , EventArgs e)
    {
      double s = 0 , R = 0 , k = 0;
      try
      {
        s = Double.Parse(textBox_S.Text);
        R = Double.Parse(textBox_R.Text);
        k = Double.Parse(textBox_KFactor.Text);

        if (radioButtonAirBending.Checked == true)
        {
          pos = 0;
        }
        else
          if (radioButtonBA.Checked == true)
          {
            pos = 1;
          }
          else
          {
            pos = 2;
          }

        if ((s > 0) && (R > 0) && (k >= 0.0))
        {

          if ((k > 1) && (radioButtonAirBending.Checked == true))
          {
            throw new FormatException();
          }

          m_S = s;
          m_R = R;
          m_kF = k;

          if (radioButtonBD.Checked == true)
          {
            double ba = 2 * (R + s) - k;
            if ((ba > (Math.PI / 2.0) * (R + s)) || (ba < (Math.PI / 2.0) * R))
            {
              Application.ShowAlertDialog("BD Out of Range !");
              textBox_KFactor.Text = "";
              textBox_KFactor.Focus();
              throw new FormatException();
            }
            else
            {
              m_kF = ((ba / (Math.PI / 2.0)) - R) / s;
              MessageBox.Show(m_kF.ToString());
            }
          }

          DialogResult = DialogResult.OK;
        }
        else
        {
          throw new FormatException();
        }
      }
      catch (FormatException)
      {
        if (s <= 0)
        {
          MessageBox.Show("S <= 0 !" , "ERROR");
          textBox_S.Text = "";
          textBox_S.Focus();
        }
        if (R <= 0)
        {
          MessageBox.Show("R <= 0 !" , "ERROR");
          textBox_R.Text = "";
          textBox_R.Focus();
        }
        if (((k < 0) || (k > 1)) && (radioButtonAirBending.Checked == true))
        {
          MessageBox.Show("K - Factor = ? ( between 0 and 1 ) !" , "ERROR");
          textBox_KFactor.Text = "";
          textBox_KFactor.Focus();
        }

        if ((k < 0) && (radioButtonBA.Checked == true))
        {
          MessageBox.Show("Bend Allowance Value = ? (BA > 0) !" , "ERROR");
          textBox_KFactor.Text = "";
          textBox_KFactor.Focus();
        }

        if ((k < 0) && (radioButtonBD.Checked == true))
        {
          MessageBox.Show("Bend Deduction Value = ? (BD > 0) !" , "ERROR");
          textBox_KFactor.Text = "";
          textBox_KFactor.Focus();
        }
      }
    }

    private void buttonKFHelp_Click(object sender , EventArgs e)
    {
      KFHelpWin form = new KFHelpWin();
      form.Show();
    }

    private void radioButtonBA_CheckedChanged(object sender , EventArgs e)
    {
      //label4.Text = "BA";
      labelKFactor.Text = "Bend Allowence";
    }

    private void radioButtonKF_CheckedChanged(object sender , EventArgs e)
    {
      //label4.Text = "KF";
      labelKFactor.Text = "K-Factor";
    }

    private void radioButtonBD_CheckedChanged(object sender , EventArgs e)
    {
      //label4.Text = "BD";
      labelKFactor.Text = "Bend Deduction";
    }

    private void button1_Click(object sender , EventArgs e)
    {
      BAHelpWin form = new BAHelpWin();
      form.Show();
    }

    private void button2_Click(object sender , EventArgs e)
    {
      BDHelpWin form = new BDHelpWin();
      form.Show();
    }

    private void button3_Click(object sender , EventArgs e)
    {
      double s = 0 , R = 0 , k = 0;
      try
      {
        s = Double.Parse(textBox_S.Text);
        R = Double.Parse(textBox_R.Text);
        k = Double.Parse(textBox_KFactor.Text);

        if (radioButtonAirBending.Checked == true)
        {
          pos = 0;
        }
        else
          if (radioButtonBA.Checked == true)
          {
            pos = 1;
          }
          else
          {
            pos = 2;
          }

        if ((s > 0) && (R > 0) && (k >= 0.0))
        {

          if ((k > 1) && (radioButtonAirBending.Checked == true))
          {
            throw new FormatException();
          }

          m_S = s;
          m_R = R;
          m_kF = k;

          if (radioButtonBD.Checked == true)
          {
            double ba = 2 * (R + s) - k;
            if ((ba > (Math.PI / 2.0) * (R + s)) || (ba < (Math.PI / 2.0) * R))
            {
              Application.ShowAlertDialog("BD Out of Range !");
              textBox_KFactor.Text = "";
              textBox_KFactor.Focus();
              throw new FormatException();
            }
            else
            {
              m_kF = ((ba / (Math.PI / 2.0)) - R) / s;
              MessageBox.Show(m_kF.ToString());
            }
          }

          DialogResult = DialogResult.OK;
        }
        else
        {
          throw new FormatException();
        }
      }
      catch (FormatException)
      {
        if (s <= 0)
        {
          MessageBox.Show("S <= 0 !" , "ERROR");
          textBox_S.Text = "";
          textBox_S.Focus();
        }
        if (R <= 0)
        {
          MessageBox.Show("R <= 0 !" , "ERROR");
          textBox_R.Text = "";
          textBox_R.Focus();
        }
        if (((k < 0) || (k > 1)) && (radioButtonAirBending.Checked == true))
        {
          MessageBox.Show("K - Factor = ? ( between 0 and 1 ) !" , "ERROR");
          textBox_KFactor.Text = "";
          textBox_KFactor.Focus();
        }

        if ((k < 0) && (radioButtonBA.Checked == true))
        {
          MessageBox.Show("Bend Allowance Value = ? (BA > 0) !" , "ERROR");
          textBox_KFactor.Text = "";
          textBox_KFactor.Focus();
        }

        if ((k < 0) && (radioButtonBD.Checked == true))
        {
          MessageBox.Show("Bend Deduction Value = ? (BD > 0) !" , "ERROR");
          textBox_KFactor.Text = "";
          textBox_KFactor.Focus();
        }
      }
    }

    private void Unfold3dForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        // The user pressed ESC - then close the form.
        if (e.KeyChar == (char)27)
        {
            Close();
        }
    }
  }
}
