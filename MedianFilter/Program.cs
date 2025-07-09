PixelRGB[,] testImage = new PixelRGB[,]
{
    { new PixelRGB(255, 255, 255), new PixelRGB(53, 12, 11), new PixelRGB(255, 255, 5) },
    { new PixelRGB(1, 2, 3), new PixelRGB(55, 55, 55), new PixelRGB(48, 70, 51) },
    { new PixelRGB(43, 125, 222), new PixelRGB(8, 0, 4), new PixelRGB(8, 3, 255) }
};

for (int i = 0; i < testImage.GetLength(0); i++)
{
    for (int j = 0; j < testImage.GetLength(1); j++)
        Console.Write("| R: {0}, G: {1}, B: {2}  | ", 
                    testImage[i, j].R, testImage[i, j].G, testImage[i, j].B);
    Console.WriteLine();
}

PixelRGB[,] filteredImage = Filtering.MedianFilter(testImage, 3);

for (int i = 0; i < filteredImage.GetLength(0); i++)
{
    for (int j = 0; j < filteredImage.GetLength(1); j++)
        Console.Write("| R: {0}, G: {1}, B: {2}  | ",
                    filteredImage[i, j].R, filteredImage[i, j].G, filteredImage[i, j].B);
    Console.WriteLine();
}

struct PixelRGB
{
    private byte red;
    private byte green;
    private byte blue;
    public byte R => red;
    public byte G => green;
    public byte B => blue;

    public PixelRGB(byte r, byte g, byte b) { red = r; green = g; blue = b; }
}

class Filtering
{
    private static byte[,] GetGrayscale(PixelRGB[,] image)
    {
        byte[,] grayscale = new byte[image.GetLength(0), image.GetLength(1)];
        for (int i = 0; i < image.GetLength(0); i++)
            for (int j = 0; j < image.GetLength(1); j++)
                grayscale[i, j] = (byte)(0.298936021293775 * image[i, j].R
                                            + 0.587043074451121 * image[i, j].G
                                                + 0.114020904255103 * image[i, j].B);
        return grayscale;
    }
    private static List<(byte, PixelRGB)> GetWindow(PixelRGB[,] image, byte[,] grayscale, int centerX, int centerY, int filterScale)
    {
        List<(byte, PixelRGB)> window = new List<(byte, PixelRGB)>();
        List<(int,int)> indices = new List<(int, int)>();
        // Так как мы точно знаем, что filterScale - нечетное число,
        // то мы можем индексировать квадрат около 0 и перемещать его к конкретному пикселю
        for (int i = -(int)filterScale / 2; i <= (int)filterScale / 2; i++)
            for (int j = -(int)filterScale / 2; j <= (int)filterScale / 2; j++)
            {
                int X = centerX + i, Y = centerY + j;
                if (X > -1 && Y > -1 && X < image.GetLength(0) && Y < image.GetLength(1))
                    indices.Add((X, Y));
            }
        indices.ForEach(index => window.Add((grayscale[index.Item1, index.Item2],
                                             image[index.Item1, index.Item2])));
        return window;
    }

    // Согласно заданию нужно размерностью 3x3 пикселя, сделал универсальный
    public static PixelRGB[,] MedianFilter(PixelRGB[,] image, int filterScale)
    {
        if (filterScale % 2 == 0) throw new ArgumentException("Размерность фильтра должна быть нечетной");
        // Преобразуем в градиент серого для однозначной оценки каждого пикселя  
        // Конверсия как в MATLAB (вариация ITU-R Rec. 601)  
        byte[,] grayscale = GetGrayscale(image);

        PixelRGB[,] filtered = new PixelRGB[image.GetLength(0), image.GetLength(1)];
        // Преобразуем пиксели согласно алгоритму фильтрации 
        // Для крайних позиций мы используем их оставшееся окружение (4 пикселя для углов и 6 для краев)
        for (int i = 0; i < image.GetLength(0); i++)
            for (int j = 0; j < image.GetLength(1); j++)
            {
                List<(byte, PixelRGB)> window = GetWindow(image, grayscale, i, j, filterScale);

                // На вход он принимает 9 значений (пикселей), а на выход выдаёт одно 
                // Медианой в четного числа пикселей выбирается меньший пиксель,
                // поскольку нужно выбрать одно значение и медиана как усредненное центральных значений здесь не применима
                window = window.OrderBy(p => p.Item1).ToList();
                int median = window.Count % 2 == 0 ? window.Count / 2 - 1 : (int)window.Count / 2;
                filtered[i, j] = window[median].Item2;
            }

        return filtered;
    }
}
