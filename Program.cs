using EasyObjectPool;
using System;
using System.Numerics;

namespace Pool
{
    /// <summary>
    /// ExampleClass
    /// </summary>
    class Rectangle
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public void Print()
        {
            Console.WriteLine($"Width: {Width}, Height: {Height}");
        }
    }

    public class Program
    {

        public static void Main()
        {
            #region Not_Use_UsingStatement
            Console.WriteLine("\n** Not Use Using Statement **");
            // usingステートメントを使わないプール
            // プールを作成する
            var objectPool = new ObjectPool<Rectangle>(() => new Rectangle(0, 0));

            // プールからオブジェクトを借りて使用する
            objectPool.Borrow(out var rectangle1);
            objectPool.Borrow(out var rectangle2);
            rectangle1.Width = 10;
            rectangle1.Height = 20;
            rectangle2.Width = 30;
            rectangle2.Height = 40;
            rectangle1.Print();
            rectangle2.Print();

            // プールにオブジェクトを返す
            objectPool.Return(rectangle1);
            objectPool.Return(rectangle2);

            // プールを明示的に解放する
            objectPool.Dispose();

            #endregion

            #region Use_UsingStatement
            Console.WriteLine("\n** Use Using Statement **");
            // usingステートメントを使うプール
            // プールを作成する
            using var objPool = new ObjectPool<Rectangle>(() => new Rectangle(0, 0));

            // プールからオブジェクトを借りて使用する
            objPool.Borrow(out var rectangleA);
            objPool.Borrow(out var rectangleB);
            rectangleA.Width = 50;
            rectangleA.Height = 100;
            rectangleB.Width = 300;
            rectangleB.Height = 1000;
            rectangleA.Print();
            rectangleB.Print();

            // プールにオブジェクトを返す
            objPool.Return(rectangle1);
            objPool.Return(rectangle2);

            // プールをusingステートメントで開放する
            #endregion

            Console.WriteLine("\n\n===== IsDispose =====");
            Console.WriteLine($"objectPool : {objectPool.IsDispose}");
            Console.WriteLine($"objPool : {objPool.IsDispose}");
            Console.WriteLine("\n※UsingStatementはスコープを抜けるまで開放されないので注意");
        }
    }
}