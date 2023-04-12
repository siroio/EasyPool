# EasyPool

* ObjectPool.csをコピーして使用してください。

## 使い方

```C#
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
```

## プールのコレクションを変えたい場合
```C#
public interface IPoolCollection<T>
{
    void SetCapacity(int capacity);
    int GetCount();
    void Push(T value);
    T Pop();
    void Clear();
}
```
上記を継承して変更ができます。（標準でStackとLinkedListが実装されています）
