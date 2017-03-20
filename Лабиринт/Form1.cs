using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gif.Components;

namespace Лабиринт
{
    public partial class Form1 : Form
    {
        int WALL = -1;      // Стена
        int CELL = 0;       // Пустая клетка
        int VISITED = 1;    // Проверенная клетка
        int VISITED2 = 2;   // Проверяемая клетка
        int VISITED3 = 3;   // Тупик
        int FINISH = 4;     // Финиш
        int START = 5;      // Старт

        public Form1()
        {
            InitializeComponent();
        }
        AnimatedGifEncoder gif_en;
        private void button1_Click(object sender, EventArgs e)
        {
            int len = (int)(numericUpDown1.Value * 2) + 1; // Вычисляем размер для массива учитывая стены
            int[,] labirint = new int[len, len]; // Создаём массив с лабиринтом

            for(int i = 0;i<len;i=i+2)          //
                for (int u = 0; u < len; u++)   // Создаём стены, по примеру картинки :
                {                               // https://habrastorage.org/files/ed2/eb4/b56/ed2eb4b5618b46e085279c1c11e380f9.png
                    labirint[i, u] = WALL;      //
                    labirint[u, i] = WALL;      //
                }                               //


            int lenbmp = (int)(len * numericUpDown2.Value); // Вычисляем размер для будующей картинки

            if (radioButton3.Checked != true)
            {
                gif_en = new AnimatedGifEncoder();
                String outputFilePath = "Генерация.gif";
                gif_en.Start(outputFilePath);
                gif_en.SetDelay(40);
                gif_en.SetRepeat(0);
                for_gif = new Bitmap(lenbmp, lenbmp);
                if(radioButton4.Checked == false)
                    AddToGif(labirint, null);
            }

            StartGenerate(ref labirint); // Запускаем генерацию лабиринта
            if (radioButton4.Checked == true)
                AddToGif(labirint, null);
            if (checkBox2.Checked == true)
            {
                int rnd_x = rnd.Next((int)len/2, len);
                int rnd_y = rnd.Next((int)len / 2, len);
                while (labirint[rnd_x, rnd_y] != VISITED || NeighbourNoWall(labirint, new Cell(rnd_x, rnd_y), VISITED).Count != 1)
                {
                    rnd_x = rnd.Next((int)len / 2, len);
                    rnd_y = rnd.Next((int)len / 2, len);
                }
                labirint[rnd_x, rnd_y] = FINISH; // Устанавливаем точку финиша
                if (radioButton3.Checked != true)
                    AddToGif(labirint, new Cell(rnd_x, rnd_y));
            }
            else
            {
                labirint[len-2, len - 2] = FINISH;
                if (radioButton3.Checked != true)
                    AddToGif(labirint, new Cell(len - 2, len - 2));
            }

            GenerateExit(ref labirint); // Ищем решение для лабиринта
            if (radioButton3.Checked == false)
                gif_en.Finish();
            Bitmap bmp = new Bitmap(lenbmp, lenbmp); // Создаём новую картинку с вычисленными размерами
            Graphics gr = Graphics.FromImage(bmp); 
            int lenline = (int)numericUpDown2.Value; // Указываем ширину стен
            for (int i = 0;i<len;i++) // Рисуем лабиринт с решением
                for(int u = 0;u<len;u++)
                    if (labirint[i, u] == WALL)
                        gr.FillRectangle(Brushes.Black, new Rectangle(lenline*i, lenline * u, lenline, lenline));
                    else if (labirint[i,u]== VISITED) 
                        gr.FillRectangle(Brushes.White, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else if (labirint[i, u] == VISITED2)
                        gr.FillRectangle(Brushes.Green, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else if (labirint[i, u] == START)
                        gr.FillRectangle(Brushes.Lime, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else if (labirint[i, u] == FINISH)
                        gr.FillRectangle(Brushes.Red, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else
                        gr.FillRectangle(Brushes.White, new Rectangle(lenline * i, lenline * u, lenline, lenline));

            bmp.Save("Решенный (Без тупиков).png",System.Drawing.Imaging.ImageFormat.Png); // Сохраняем в файл
            for (int i = 0; i < len; i++) // Рисуем лабиринт с решением
                for (int u = 0; u < len; u++)
                    if (labirint[i, u] == VISITED3)
                        gr.FillRectangle(Brushes.Orange, new Rectangle(lenline * i, lenline * u, lenline, lenline));
            bmp.Save("Решенный (С тупиками).png", System.Drawing.Imaging.ImageFormat.Png);

            for (int i = 0; i < len; i++) // Рисуем лабиринт без решения
                for (int u = 0; u < len; u++)
                    if (labirint[i, u] == WALL)
                        gr.FillRectangle(Brushes.Black, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else if (labirint[i, u] == VISITED || labirint[i, u] == VISITED2)
                        gr.FillRectangle(Brushes.White, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else if (labirint[i, u] == START)
                        gr.FillRectangle(Brushes.Lime, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else if (labirint[i, u] == FINISH)
                        gr.FillRectangle(Brushes.Red, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                    else
                        gr.FillRectangle(Brushes.White, new Rectangle(lenline * i, lenline * u, lenline, lenline));
            bmp.Save("Не Решенный.png", System.Drawing.Imaging.ImageFormat.Png); // Сохраняем в файл
        }

        Random rnd = new Random(); // Глобальная переменная рандома, что бы не повторялись значения
        Stack<Cell> st = new Stack<Cell>(); // Стэк

        Bitmap for_gif;

        void AddToGif(int[,] labirint,Cell change)
        {
            if (radioButton3.Checked == true)
                return;
            int len = (int)(numericUpDown1.Value * 2) + 1;
            int lenbmp = (int)(len * numericUpDown2.Value);
            Graphics gr = Graphics.FromImage(for_gif);
            int lenline = (int)numericUpDown2.Value;
            if(change == null)
            {
                for (int i = 0; i < len; i++)
                    for (int u = 0; u < len; u++)
                        if (labirint[i, u] == WALL)
                            gr.FillRectangle(Brushes.Black, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                        else if (labirint[i, u] == VISITED)
                            gr.FillRectangle(Brushes.White, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                        else if (labirint[i, u] == VISITED2)
                            gr.FillRectangle(Brushes.Green, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                        else if (labirint[i, u] == VISITED3)
                            gr.FillRectangle(Brushes.Orange, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                        else if (labirint[i, u] == START)
                            gr.FillRectangle(Brushes.Lime, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                        else if (labirint[i, u] == FINISH)
                            gr.FillRectangle(Brushes.Red, new Rectangle(lenline * i, lenline * u, lenline, lenline));
                        else
                            gr.FillRectangle(Brushes.White, new Rectangle(lenline * i, lenline * u, lenline, lenline));
            }
            else
            {
                if (labirint[change.x, change.y] == WALL)
                    gr.FillRectangle(Brushes.Black, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
                else if (labirint[change.x, change.y] == VISITED)
                    gr.FillRectangle(Brushes.White, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
                else if (labirint[change.x, change.y] == VISITED2)
                    gr.FillRectangle(Brushes.Green, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
                else if (labirint[change.x, change.y] == VISITED3)
                    gr.FillRectangle(Brushes.Orange, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
                else if (labirint[change.x, change.y] == START)
                    gr.FillRectangle(Brushes.Lime, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
                else if (labirint[change.x, change.y] == FINISH)
                    gr.FillRectangle(Brushes.Red, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
                else
                    gr.FillRectangle(Brushes.White, new Rectangle(lenline * change.x, lenline * change.y, lenline, lenline));
            }
            gif_en.AddFrame(for_gif);
        }

        private void GenerateExit(ref int[,] labirint)
        {
            Cell StartCell = new Cell(1, 1); // Устанавливаем на старт
            Cell CurrectCell = StartCell; // Делаем старт - текущим положением
            st.Push(CurrectCell); // Добавляем текущее расположение в стэк
            labirint[1, 1] = START; // Делаем точку стартом
            while(labirint[CurrectCell.x, CurrectCell.y] != FINISH) // Повторяем до тех пор, пока не окажимся на финише
            {
                List<Cell> Neighbours = NeighbourNoWall(labirint, CurrectCell, VISITED); // Выбираем все клетки, в которых нас не было
                if(Neighbours.Count == 0) 
                {
                    // Если не посещённых клеток нету, то мы в тупике, значит тут финиша нету, возвращаемся назад помечая данную местность тупиком
                    labirint[CurrectCell.x, CurrectCell.y] = VISITED3;
                    AddToGif(labirint, CurrectCell);
                    CurrectCell = st.Pop();
                }
                else
                {
                    // Если есть не посещённые клетки, то идём в них, а старую отправляем в стэк.
                    Cell NewCell = Neighbours[rnd.Next(Neighbours.Count)];
                    st.Push(CurrectCell);
                    AddToGif(labirint, CurrectCell);
                    CurrectCell = NewCell;
                    if(labirint[CurrectCell.x, CurrectCell.y] != FINISH)
                        labirint[CurrectCell.x, CurrectCell.y] = VISITED2;
                }
            }
        }

        

        private void StartGenerate(ref int[,] labirint)
        {
            int len = (int)(numericUpDown1.Value * 2);
            int rnd_x = rnd.Next(1, len);
            int rnd_y = rnd.Next(1, len);
            while (labirint[rnd_x, rnd_y] != CELL)
            {
                rnd_x = rnd.Next(1, len);
                rnd_y = rnd.Next(1, len);
            }
            Cell StartCell = new Cell(rnd_x, rnd_y); // Устанавливаем в начало
            Cell CurrectCell = StartCell; // Делаем начало - текущим положением
            while (ExistCells(labirint)) // Выполняем до тех пор, пока есть не пройденные клетки
            {
                List<Cell> Neighbours = Neighbour(labirint, CurrectCell); // Ищём соседей, которых не проходили
                if(Neighbours.Count>0) 
                {
                    // Если соседи есть, то выбираем случайного из них и убираем между текущим положением и соседом стенку
                    Cell NewCell = Neighbours[rnd.Next(Neighbours.Count)];  //Как это выглядит :
                    RemoveWall(ref labirint, CurrectCell, NewCell);         // https://habrastorage.org/files/663/eee/d16/663eeed16b21499890a583150ba5d220.png
                    if (Neighbours.Count>=2)                                // https://habrastorage.org/files/2a9/8bc/aff/2a98bcaff2e64e31a77aa1479eb7cd72.png
                    {                                                       // https://habrastorage.org/files/67d/9bd/29f/67d9bd29fbac4398801134179116405f.png
                        if(radioButton1.Checked == true)
                            qe.Enqueue(CurrectCell);
                        else
                            st.Push(CurrectCell);
                    }
                    CurrectCell = NewCell;
                }
                else if (radioButton1.Checked == true)
                    CurrectCell = qe.Dequeue();
                else
                    CurrectCell = st.Pop();
            }
            if (radioButton1.Checked == true)
                qe.Clear();
            else
                st.Clear();
        }

        Queue<Cell> qe = new Queue<Cell>(); // Очередь

        private bool ExistCells(int[,] labirint) // Функция которая проверяет, все ли ячейки посещенны
        {
            int len = (int)(numericUpDown1.Value * 2);
            for (int i = 0; i < len; i++)
                for (int u = 0; u < len; u++)
                    if (labirint[i, u] == CELL)
                        return true;
            return false;

        }

        private List<Cell> Neighbour(int[,] labirint,Cell cell,int val = 0) // Ищем соседей сверху, снизу, справа, слева.
        {
            List<Cell> neighbours = new List<Cell>();
            int len = (int)(numericUpDown1.Value * 2);
            int x = cell.x;
            int y = cell.y;
            if (x>1)
                if(labirint[x-2,y] == val)
                    neighbours.Add(new Cell(x - 2, y));
            if(y>1)
                if (labirint[x, y-2] == val)
                    neighbours.Add(new Cell(x, y-2));
            if(y< len-2)
                if (labirint[x, y + 2] == val)
                    neighbours.Add(new Cell(x, y + 2));
            if (x < len - 2)
                if (labirint[x + 2, y] == val)
                    neighbours.Add(new Cell(x + 2, y));
            return neighbours;
        }

        private List<Cell> NeighbourNoWall(int[,] labirint, Cell cell, int val = 0) // Ищем соседей сверху, снизу, справа, слева. (Без стены)
        {
            List<Cell> neighbours = new List<Cell>();
            int x = cell.x;
            int y = cell.y;
            if (labirint[x - 1, y] == val || labirint[x - 1, y] == FINISH)
                neighbours.Add(new Cell(x - 1, y));
            if (labirint[x, y - 1] == val || labirint[x, y - 1] == FINISH)
                neighbours.Add(new Cell(x, y - 1));
            if (labirint[x, y + 1] == val || labirint[x, y + 1] == FINISH)
                neighbours.Add(new Cell(x, y + 1));
            if (labirint[x + 1, y] == val || labirint[x + 1, y] == FINISH)
                neighbours.Add(new Cell(x + 1, y));
            return neighbours;
        }

        private void RemoveWall(ref int[,] labirint,Cell Start,Cell End) // Просто удаляем стену
        {
            if(Start.x == End.x)
            {
                if(Start.y>End.y)
                {
                    labirint[Start.x, Start.y] = VISITED;
                    labirint[Start.x, Start.y - 1] = VISITED;
                    if(radioButton5.Checked == true)
                        AddToGif(labirint, new Cell(Start.x, Start.y - 1));
                    labirint[Start.x, Start.y - 2] = VISITED;
                }
                else
                {
                    labirint[Start.x, Start.y] = VISITED;
                    labirint[Start.x, Start.y + 1] = VISITED;
                    if (radioButton5.Checked == true)
                        AddToGif(labirint, new Cell(Start.x, Start.y + 1));
                    labirint[Start.x, Start.y + 2] = VISITED;
                }
            }
            else
            {
                if (Start.x > End.x)
                {
                    labirint[Start.x, Start.y] = VISITED;
                    labirint[Start.x - 1, Start.y] = VISITED;
                    if (radioButton5.Checked == true)
                        AddToGif(labirint, new Cell(Start.x - 1, Start.y));
                    labirint[Start.x - 2, Start.y] = VISITED;
                }
                else
                {
                    labirint[Start.x, Start.y] = VISITED;
                    labirint[Start.x + 1, Start.y] = VISITED;
                    if (radioButton5.Checked == true)
                        AddToGif(labirint, new Cell(Start.x + 1, Start.y));
                    labirint[Start.x + 2, Start.y] = VISITED;
                }
            }
        }
    }
}

class Cell
{
    public Cell(int X,int Y)
    {
        x = X;
        y = Y;
    }
    public int x;
    public int y;
}