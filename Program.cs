//寫一個C# CONSOLE的程式碼
//1.場景為 寬20字元，長50行
//2.場景外圍有*號包圍
//3.每一行最右側的*號之後會顯示行數
//4.行數會以每一秒增加1推進表示該地圖有在前行
//5.最底下的星號前一行的第10個字元會有一個^符號當作是飛船
//6.可以利用方向鍵控制^左右移動
//7.場上會隨機出現障礙物x且會往底下靠近
//8.碰到x號結束遊戲

//以上問題依序完成
//需要可以清楚解釋各部分程式碼
//能寫到哪算到哪

//Deadline: 2023 / 5 / 9
//繳交形式: Github / GitLab / ... 任何您方便繳交的形式即可



//我使用座標紀錄地圖上的隕石與飛船，然後每秒刷新

using System.Diagnostics;
using System.Text;

//調整主控台寬高，限 "Windows"
Console.WindowHeight = Console.LargestWindowHeight;
Console.WindowWidth = 100;
Console.Clear();
Console.CursorVisible = false;

//基礎值設定
int width = 20;
int length = 50;
int cycle = 500;

string[,] strMap2D = new string[length + 2, width + 3];
int mapX = strMap2D.GetUpperBound(1);
int mapY = strMap2D.GetUpperBound(0);
Dictionary<string, int> borderInside = new Dictionary<string, int>()  //再往內縮一個單位
{
    {"Up" , 1 },
    {"Down" , mapY - 1 },
    {"Left" , 1 },
    {"Right" , mapX - 2 },
};
int starShipX = mapX / 2;
int starShipY = mapY - 1;

bool gameover = true;
bool stop = true;
StringBuilder sbMap = new StringBuilder();
CancellationTokenSource source = new CancellationTokenSource();
Random rnd = new Random();
Stopwatch stopWatch = new Stopwatch();

//遊戲說明
Console.WriteLine("Game Instructions:");
Console.WriteLine();
Console.WriteLine("1. Press Space Bar To Stop");
Console.WriteLine();
Console.WriteLine("2. Press Up Down Left Right To Move");
Console.WriteLine();
Console.WriteLine("Now, You Can Press Enter Start Game");
Console.ReadLine();
Console.Clear();

//初始畫面
Initial();
sbMap.Clear();
MapToSB();
ReplaceConsoleOfSB();
var enter = new ConsoleKey();

//測試用
//int i = 0;
//WriteAt(Thread.CurrentThread.ManagedThreadId.ToString()!, 0, mapY+5+i++);
//Stopwatch stopWatch = Stopwatch.StartNew();  //開始時間
//Console.WriteLine(stopWatch.ElapsedMilliseconds + "ms");

//開始遊戲
Thread.Sleep(100);
do
{
    //暫停
    if (!stop)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteAt("STOP", mapX / 2 - 2, mapY / 2 + 1);
        WriteAt("STOP", mapX / 2 - 2, mapY / 2);
        WriteAt("STOP", mapX / 2 - 2, mapY / 2 - 1);
        Console.ForegroundColor = ConsoleColor.White;
        if (Console.ReadKey().Key == ConsoleKey.Spacebar)
        {
            WriteAt("    ", mapX / 2 - 2, mapY / 2 + 1);
            WriteAt("    ", mapX / 2 - 2, mapY / 2);
            WriteAt("    ", mapX / 2 - 2, mapY / 2 - 1);
            ConsoleMeteorite();
            StarShipConsole();
            stop = true;
            source = new CancellationTokenSource();
        }
    }

    //非同步讀取輸入，上下左右 & 暫停
    _ = Task.Run(() =>
    {
        while (stop & gameover)
        {
            enter = Console.ReadKey(true).Key;
            switch (enter)
            {
                case ConsoleKey.RightArrow:
                    //設定界線
                    if (starShipX < borderInside["Right"])
                    {
                        StarShipMove("x",1);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    //設定界線
                    if (starShipX > borderInside["Left"])
                    {
                        StarShipMove("x", -1);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    //設定界線
                    if (starShipY > borderInside["Up"])
                    {
                        StarShipMove("y", -1);
                    }
                    break;
                case ConsoleKey.DownArrow:
                    //設定界線
                    if (starShipY < borderInside["Down"])
                    {
                        StarShipMove("y", 1);
                    }
                    break;
                case ConsoleKey.Spacebar:
                    //暫停
                    source.Cancel();
                    stopWatch.Stop();
                    stop = false;
                    break;
            }
            enter = new ConsoleKey();   //清除按的Key
        };
    }
    , source.Token);

    //畫面往前
    while (stop & gameover)
    {
        Console.CursorVisible = false;  //移除滑鼠不小心按到的游標
        if (await time())     //因為暫停跳出迴圈就不執行並暫停時間
        {
            stopWatch.Reset();
            GoAhead();
            sbMap.Clear();
            MapToSB();
            ReplaceConsoleOfSB();
            StarShipConsole();
            if (!gameover)
                break;
        }
    }

} while (gameover);

//GAME OVER之後
Thread.Sleep(100);
Console.ForegroundColor = ConsoleColor.Red;
WriteAt("Game Over", mapX / 2 - 4, mapY / 2 - 2);
WriteAt("And", mapX / 2 - 1, mapY / 2);
WriteAt("Press Enter", mapX / 2 - 5, mapY / 2 + 2);
Console.ForegroundColor = ConsoleColor.White;
Console.SetCursorPosition(0, mapY + 1);
Console.ReadLine();
source.Dispose();

void StarShipMove(string axis, int move)
{
    //判斷x軸、y軸
    if (axis == "x")
    {
        //判斷有沒有撞隕石
        if (strMap2D[starShipY, starShipX + move] == "X")
        {
            gameover = false;
            source.Cancel();
        }
        WriteAt(" ", starShipX, starShipY);
        starShipX += move;
        StarShipConsole();
    }
    else
    {
        //判斷有沒有撞隕石
        if (strMap2D[starShipY + move, starShipX] == "X")
        {
            gameover = false;
            source.Cancel();
        }
        WriteAt(" ", starShipX, starShipY);
        starShipY += move;
        StarShipConsole();
    }
}

void ConsoleMeteorite()
{
    for (int y = mapY - 1; y >= 1; y--)
    {
        //去掉左右外圍星號、數字
        for (int x = mapX - 2; x >= 1; x--)
        {
            if (strMap2D[y, x] == "X")
            {
                WriteAt("X", x, y);
            }
        }
    }
}

async Task<bool> time()
{
    await Task.Run(() =>
    {
        stopWatch.Start();
        while (stopWatch.ElapsedMilliseconds < cycle & stop & gameover) { }
    }, source.Token);

    if (stop & gameover)
        return true;
    else
        return false;
}

void ReplaceConsoleOfSB()
{
    WriteAt(sbMap.ToString(), 0, 0);
}

void StarShipConsole()
{
    WriteAt("^", starShipX, starShipY);
}

void WriteAt(string s, int x, int y)
{
    Console.SetCursorPosition(x, y);
    Console.Write(s);
}

//效能較差，故無用
void ReplaceConsole()
{
    for (int y = mapY; y >= 0; y--)
    {
        for (int x = mapX; x >= 0; x--)
        {
            //看飛船座標並印出，找很久才發現沒有印出飛船
            if (x == starShipX & y == starShipY)
                StarShipConsole();

            //讓空白的都不要再打印
            else 
            {
                if (strMap2D[y, x] == " ")
                    if (strMap2D[y + 1, x] == "X" | strMap2D[y + 1, x] == "*")
                        WriteAt(" ", x, y);   //下面有隕石或邊界就給他空白
                    else
                    { }  //空白就不用印了
                else
                    WriteAt(strMap2D[y, x], x, y);
            }
        }
    }
}

//地圖向上，畫面往下移動
void GoAhead()
{
    //去掉上下外圍星號，且最上面(先清掉再放障礙物)，還有判斷飛船有沒有撞到
    for (int y = mapY - 1; y >= 1; y--)
    {
        //去掉左右外圍星號、數字
        for (int x = mapX - 2; x >= 1; x--)
        {
            //第一排之外都往下一個單位
            if (y == 1)
            {
                
            }
            //第二排之後，看有沒有撞到隕石
            else if (x == starShipX & y == starShipY & strMap2D[y - 1, x] == "X")
            {
                //飛船撞到
                gameover = false;
                source.Cancel();
            }
            //往下移
            else
            {
                //但忽略飛船
                if (x == starShipX & y == starShipY)
                {
                    strMap2D[y, x] = "^";
                    //StarShipConsole();
                }
                else if (strMap2D[y - 1, x] == "^")
                {
                    //位置上面是飛船，這個位置就略過 
                }
                else
                    strMap2D[y, x] = strMap2D[y - 1, x];
            }
        }
    }

    //第一排，出現隕石，並判斷有沒有撞到飛船
    //清掉第一排放置的障礙物，除了飛船
    for (int x = mapX - 2; x >= 1; x--)
    {
        if (starShipX ==  x & starShipY == 1)
            strMap2D[1, x] = "^";
        else
            strMap2D[1, x] = " ";
    }
    //出現障礙物，並判斷障礙物出現的地方有沒有撞到飛船
    int meteoriteX = rnd.Next(0, mapX * 3);
    if (meteoriteX < mapX - 2 & meteoriteX > 1)
    {
        strMap2D[1, meteoriteX] = "X";
        if (starShipX == meteoriteX & starShipY == 1)
        {
            gameover = false;
            source.Cancel();
        }
    }

    //數字改變
    int xNum = mapX;  //Pass by Value
    for (int y = mapY; y >= 0; y--)
    {
        strMap2D[y, xNum] = (int.Parse(strMap2D[y, xNum]) + 1).ToString();
    }
}

void MapToSB()
{
    foreach (string c in strMap2D)
    {
        sbMap.Append(c);
        if (int.TryParse(c.ToString(), out int n))
        {
            sbMap.Append("\n");
        }
    }
}

void Initial()
{
    int num = 0;
    for (int y = mapY; y >= 0; y--)
    {
        for (int x = mapX; x >= 0; x--)
        {
            if (x == mapX)  //最右邊數字
            {
                strMap2D[y, x] = num.ToString();
                num++;
            }
            else if (x == 0 | x == mapX - 1 | y == 0 | y == mapY)   //|如果前面是true則不會評估後面
            {
                strMap2D[y, x] = "*";
            }
            else
            {
                strMap2D[y, x] = " ";
            }
        }
    }
    strMap2D[starShipY, starShipX] = "^";
}