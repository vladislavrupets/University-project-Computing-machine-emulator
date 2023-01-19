using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;


namespace EM3
{
    /// <summary>
    /// Логика взаимодействия для CodeWin.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorkWithFile workWithFile;
        private List<TableRow> tableList;
        private Commands commands;
        private Registers registers;
        public MainWindow()
        {
            InitializeComponent();
            registers = new Registers();
            commands = new Commands(registers);
            ViewCommands viewCommands = new ViewCommands(commands, this);
            ViewRegisters viewRegister = new ViewRegisters(registers, this);
            workWithFile = new WorkWithFile();
            tableList = new List<TableRow>();
        }

        private void codeDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            tableList.Clear();
            for (int i = 1; i < 513; i++)
            {
                tableList.Add(new TableRow(i.ToString(), "ПЕР", "000", "000"));
            }
            codeDataGrid.ItemsSource = tableList;
            List<Registers> regs = new List<Registers>();
            regs.Add(registers);
            registersList.ItemsSource = regs;
            registersList2.ItemsSource = regs;
        }
        private void startAgain()
        {
            registers.ResetRegisters();
            inputLb1.Items.Clear();
            outputLb.Items.Clear();
            diagnosticsLb2.Text = "";
            stepcounter = 0;
        }
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            startAgain();
            int counter = 0;
            const int maxCounter = 1000;
            bool progStarted = true;
            while (progStarted)
            {
                try
                {
                    if (counter >= maxCounter || registers.RA >= 512)
                    {
                        registers.Err = 1;
                        registers.regsChanged();
                        throw new ArgumentException(String.Format("Не найдена операция выхода из программы.\n"));
                    }
                    switch (tableList[registers.RA - 1].command)
                    {
                        case "ПЕР":
                            if (decimal.Parse(tableList[registers.RA - 1].a2) - 1 > 0)
                            {                              
                                commands.MoveOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            }
                            else
                                commands.MoveOp(tableList[registers.RA - 1].a1, "000");
                            registers.RA++; registers.RK = 0; counter++;
                            break;
                        case "СЛВ":
                            commands.AddFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 1; counter++;
                            break;
                        case "ВЧВ":
                            commands.SubFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 2; counter++;
                            break;
                        case "УМВ":
                            commands.MulFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 3; counter++;
                            break;
                        case "ДЕВ":
                            commands.DivFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 4; counter++;
                            break;
                        case "ВВВ":
                            commands.InputFloatArr(tableList[registers.RA - 1].a1, tableList[registers.RA - 1].a2);
                            registers.RA++; registers.RK = 5; counter++;
                            break;
                        case "ВВЦ":
                            commands.InputIntArr(tableList[registers.RA - 1].a1, tableList[registers.RA - 1].a2);
                            registers.RA++; registers.RK = 6; counter++;
                            break;
                        case "БЕЗ":
                            commands.GoTo(tableList[registers.RA - 1].a2);
                            registers.RK = 9; counter++;
                            break;
                        case "ЦЕЛ":
                            commands.FloatToInt(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 10; counter++;
                            break;
                        case "СЛЦ":
                            commands.AddIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 11; counter++;
                            break;
                        case "ВЧЦ":
                            commands.SubIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 12; counter++;
                            break;
                        case "УМЦ":
                            commands.MulIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 13; counter++;
                            break;
                        case "ДЕЦ":
                            commands.DivIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 14; counter++;
                            break;
                        case "ВЫВ":
                            List<TableRow> outputFloat = new List<TableRow>();
                            for (int i = int.Parse(tableList[registers.RA - 1].a1) - 1; i < int.Parse(tableList[registers.RA - 1].a1) + int.Parse(tableList[registers.RA - 1].a2) - 1; i++)
                                outputFloat.Add(tableList[i]);
                            commands.PrintFloat(outputFloat);
                            registers.RA++; registers.RK = 15; counter++;
                            break;
                        case "ВЫЦ":
                            List<TableRow> outputInt = new List<TableRow>();
                            for (int i = int.Parse(tableList[registers.RA - 1].a1) - 1; i < int.Parse(tableList[registers.RA - 1].a1) + int.Parse(tableList[registers.RA - 1].a2) - 1; i++)
                                outputInt.Add(tableList[i]);
                            commands.PrintInt(outputInt);
                            registers.RA++; registers.RK = 16; counter++;
                            break;
                       
                        case "УСЛ":
                            commands.wGoTo(tableList[registers.RA - 1].a1, tableList[registers.RA - 1].a2);
                            registers.RK = 19; counter++;
                            break;
                        case "ВЕЩ":
                            commands.IntToFloat(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 20; counter++;
                            break;
                        case "МОД":
                            commands.IntDivRemainOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                                tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                            registers.RA++; registers.RK = 24; counter++;
                            break;
                        case "ИТР":
                            commands.ArrIterator(tableList[int.Parse(tableList[registers.RA - 1].a1) - 1], tableList[registers.RA - 1].a2);
                            registers.RA++; registers.RK = 30; counter++; registers.regsChanged();
                            break;
                        case "ОСТ":
                            progStarted = false;
                            registers.RK = 31; registers.R1 = 0; registers.R2 = 0; registers.regsChanged();
                            diagnosticsLb2.Text = "Программа завершена успешно!";
                            return;
                        default:
                            registers.Err = 1;
                            registers.regsChanged();
                            throw new ArgumentException(String.Format("Неизвестная команда: {0}.\n", tableList[registers.RA - 1].command));
                    }
                }
                catch (Exception exc)
                {
                    ErrorMsg(exc.Message);
                    return;
                }
            }
        }
        private void ErrorMsg(string err_msg)
        {
            diagnosticsLb2.Text += (String.Format("Программа завершена аварийно!\n"));
            diagnosticsLb2.Text += (String.Format("Строка: {0}\n", registers.RA));
            diagnosticsLb2.Text += err_msg;
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            tableList = workWithFile.OpenFile();
            codeDataGrid.ItemsSource = tableList;
            codeDataGrid.Items.Refresh();
            if (tableList.Count != 0)
                fileLb.Text = workWithFile.openFileName;
            else
                codeDataGrid_Loaded(this, null);
            inputLb1.Items.Clear();
            outputLb.Items.Clear();
            diagnosticsLb2.Text = "";
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> fileInput = new List<string>();
            foreach (var item in codeDataGrid.Items.OfType<TableRow>())
            {
                string adress = item.adress;
                string command = item.command;
                string a1 = item.a1;
                string a2 = item.a2;
                fileInput.Add(adress + " " + command + " " + a1 + " " + a2);
            }
            workWithFile.SaveFile(fileInput);
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> fileInput = new List<string>();
            foreach (var item in codeDataGrid.Items.OfType<TableRow>())
            {
                string adress = item.adress;
                string command = item.command;
                string a1 = item.a1;
                string a2 = item.a2;
                fileInput.Add(adress + " " + command + " " + a1 + " " + a2);
            }
            workWithFile.SaveAs(fileInput);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void clearTableBtn_Click(object sender, RoutedEventArgs e)
        {
            codeDataGrid_Loaded(this, null);
            codeDataGrid.Items.Refresh();
            startAgain();
        }
        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {
            HelpWin helpWin = new HelpWin();
            helpWin.Show();
        }
        private int stepcounter = 0;
        private void stepBtn_Click(object sender, RoutedEventArgs e)
        {
            const int maxCounter = 1000;
            if (stepcounter >= maxCounter || registers.RA >= 512)
            {
                registers.Err = 1;
                registers.regsChanged();
                throw new ArgumentException(String.Format("Не найдена операция выхода из программы.\n"));
            }
            try
            {
                switch (tableList[registers.RA - 1].command)
                {
                    case "ПЕР":
                        if (decimal.Parse(tableList[registers.RA - 1].a2) - 1 > 0)
                        {
                            commands.MoveOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        }
                        else
                            commands.MoveOp(tableList[registers.RA - 1].a1, "000");
                        registers.RA++; registers.RK = 0; stepcounter++; registers.regsChanged();
                        break;
                    case "СЛВ":
                        commands.AddFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 1; stepcounter++; registers.regsChanged();
                        break;
                    case "ВЧВ":
                        commands.SubFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 2; stepcounter++; registers.regsChanged();
                        break;
                    case "УМВ":
                        commands.MulFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 3; stepcounter++; registers.regsChanged();
                        break;
                    case "ДЕВ":
                        commands.DivFloatOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 4; stepcounter++; registers.regsChanged();
                        break;
                    case "ВВВ":
                        commands.InputFloatArr(tableList[registers.RA - 1].a1, tableList[registers.RA - 1].a2);
                        registers.RA++; registers.RK = 5; stepcounter++; registers.regsChanged();
                        break;
                    case "ВВЦ":
                        commands.InputIntArr(tableList[registers.RA - 1].a1, tableList[registers.RA - 1].a2);
                        registers.RA++; registers.RK = 6; stepcounter++; registers.regsChanged();
                        break;
                    case "БЕЗ":
                        commands.GoTo(tableList[registers.RA - 1].a2);
                        registers.RK = 9; stepcounter++; registers.regsChanged();
                        break;
                    case "ЦЕЛ":
                        commands.FloatToInt(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 10; stepcounter++; registers.regsChanged();
                        break;
                    case "СЛЦ":
                        commands.AddIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 11; stepcounter++; registers.regsChanged();
                        break;
                    case "ВЧЦ":
                        commands.SubIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 12; stepcounter++; registers.regsChanged();
                        break;
                    case "УМЦ":
                        commands.MulIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 13; stepcounter++; registers.regsChanged();
                        break;
                    case "ДЕЦ":
                        commands.DivIntOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 14; stepcounter++; registers.regsChanged();
                        break;
                    case "ВЫВ":
                        List<TableRow> outputFloat = new List<TableRow>();
                        for (int i = int.Parse(tableList[registers.RA - 1].a1) - 1; i < int.Parse(tableList[registers.RA - 1].a1) + int.Parse(tableList[registers.RA - 1].a2) - 1; i++)
                            outputFloat.Add(tableList[i]);
                        commands.PrintFloat(outputFloat);
                        registers.RA++; registers.RK = 15; stepcounter++; registers.R1 = 0; registers.R2 = 0; registers.regsChanged();
                        break;
                    case "ВЫЦ":
                        List<TableRow> outputInt = new List<TableRow>();
                        for (int i = int.Parse(tableList[registers.RA - 1].a1) - 1; i < int.Parse(tableList[registers.RA - 1].a1) + int.Parse(tableList[registers.RA - 1].a2) - 1; i++)
                            outputInt.Add(tableList[i]);
                        commands.PrintInt(outputInt);
                        registers.RA++; registers.RK = 16; stepcounter++; registers.R1 = 0; registers.R2 = 0; registers.regsChanged();
                        break;
                   
                    case "УСЛ":
                        commands.wGoTo(tableList[registers.RA - 1].a1, tableList[registers.RA - 1].a2);
                        registers.RK = 19; stepcounter++; registers.regsChanged();
                        break;
                    case "ВЕЩ":
                        commands.IntToFloat(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 20; stepcounter++; registers.regsChanged();
                        break;
                    case "МОД":
                        commands.IntDivRemainOp(tableList[registers.RA - 1].a1, tableList[int.Parse(tableList[registers.RA - 1].a1) - 1].a2,
                            tableList[int.Parse(tableList[registers.RA - 1].a2) - 1].a2);
                        registers.RA++; registers.RK = 24; stepcounter++; registers.regsChanged();
                        break;
                    case "ИТР":
                        commands.ArrIterator(tableList[int.Parse(tableList[registers.RA - 1].a1) -1], tableList[registers.RA - 1].a2);
                        registers.RA++; registers.RK = 30; stepcounter++; registers.regsChanged();
                        break;
                    case "ОСТ":
                        registers.RK = 31; registers.R1 = 0; registers.R2 = 0; registers.regsChanged();
                        stepBtn.IsEnabled = false;
                        diagnosticsLb2.Text = "Программа завершена успешно!"; 
                        return;
                    default:
                        registers.Err = 1;
                        registers.regsChanged();
                        throw new ArgumentException(String.Format("Неизвестная команда: {0}.\n", tableList[registers.RA - 1].command));
                }
            }
            catch (Exception exc)
            {
                ErrorMsg(exc.Message);
                return;
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            startBtn.IsEnabled = true;
            debugBtn.IsEnabled = true;
            stepBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;
            registers.T = 1;
            registers.regsChanged();
            startAgain();
        }

        private void startdebugBtn_Click(object sender, RoutedEventArgs e)
        {
            startAgain();
            registers.T = 1;
            registers.regsChanged();
            stepBtn.IsEnabled = true;
            stopBtn.IsEnabled = true;
            startBtn.IsEnabled = false;
            debugBtn.IsEnabled = false;
        }
    }
}
