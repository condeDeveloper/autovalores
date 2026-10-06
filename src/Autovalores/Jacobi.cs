namespace Conde.Autovalores;

/// <summary>
/// JACOBI, 1846: zerar um elemento de fora da diagonal por vez, com rotacoes.
///
/// O metodo e quase ingenuo e funciona: escolha o maior elemento fora da
/// diagonal, gire o plano daquelas duas coordenadas no angulo que zera
/// exatamente ele, e repita. Cada rotacao estraga um pouco os zeros anteriores,
/// e mesmo assim a soma dos quadrados de fora da diagonal CAI em todas as
/// rotacoes, sem excecao, e cai pelo dobro do quadrado do elemento zerado.
///
/// Essa quantidade que so cai e o que garante a convergencia, e ela e medivel
/// passo a passo: nao e argumento, e numero.
///
/// Ele so vale para matriz SIMETRICA, e isso nao e limitacao pequena: matriz
/// simetrica tem todos os autovalores reais e uma base de autovetores
/// ortogonais, e matriz qualquer nao tem nem uma coisa nem outra.
/// </summary>
public static class Jacobi
{
    public sealed record Resultado(double[] Autovalores, Matriz Autovetores, int Rotacoes,
                                   bool Convergiu, List<double> ForaDaDiagonal);

    public static Resultado Diagonalizar(Matriz matriz, double tolerancia = 1e-14, int limite = 1000)
    {
        ArgumentNullException.ThrowIfNull(matriz);

        if (!matriz.Simetrica())
            throw new ArgumentException("Jacobi so vale para matriz simetrica", nameof(matriz));

        var n = matriz.Ordem;
        var a = matriz.Copia();
        var v = Matriz.Identidade(n).Copia();
        var trilha = new List<double> { matriz.ForaDaDiagonal() };

        for (var rodada = 1; rodada <= limite; rodada++)
        {
            // O maior elemento fora da diagonal. Procurar o maior custa n ao
            // quadrado por rotacao e e o que da a garantia mais forte; varrer
            // em ordem fixa e mais barato e converge mais devagar.
            int melhorI = 0, melhorJ = 1;
            var maior = 0.0;

            for (var i = 0; i < n; i++)
                for (var j = i + 1; j < n; j++)
                    if (Math.Abs(a[i, j]) > maior) { maior = Math.Abs(a[i, j]); melhorI = i; melhorJ = j; }

            if (maior < tolerancia)
                return new Resultado(Ordenar(a, n), new Matriz(v), rodada - 1, true, trilha);

            Girar(a, v, melhorI, melhorJ, n);
            trilha.Add(Soma(a, n));
        }

        return new Resultado(Ordenar(a, n), new Matriz(v), limite, false, trilha);
    }

    /// <summary>
    /// A rotacao que zera exatamente o elemento escolhido.
    ///
    /// O angulo sai de uma conta fechada, e a forma usada aqui, com a tangente
    /// achada pela raiz menor, nao e capricho: a forma direta com arco tangente
    /// perde digitos quando os dois elementos da diagonal sao quase iguais, que
    /// e justamente o caso dificil.
    /// </summary>
    private static void Girar(double[,] a, double[,] v, int p, int q, int n)
    {
        var diferenca = a[q, q] - a[p, p];
        double t;

        if (Math.Abs(a[p, q]) < Math.Abs(diferenca) * 1e-36)
        {
            t = a[p, q] / diferenca;
        }
        else
        {
            var theta = diferenca / (2 * a[p, q]);
            t = Math.Sign(theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
            if (theta == 0) t = 1;
        }

        var c = 1 / Math.Sqrt(t * t + 1);
        var s = t * c;

        var app = a[p, p];
        var aqq = a[q, q];
        var apq = a[p, q];

        a[p, p] = c * c * app - 2 * s * c * apq + s * s * aqq;
        a[q, q] = s * s * app + 2 * s * c * apq + c * c * aqq;
        a[p, q] = 0;
        a[q, p] = 0;

        for (var k = 0; k < n; k++)
        {
            if (k == p || k == q) continue;

            var akp = a[k, p];
            var akq = a[k, q];

            a[k, p] = c * akp - s * akq;
            a[p, k] = a[k, p];
            a[k, q] = s * akp + c * akq;
            a[q, k] = a[k, q];
        }

        for (var k = 0; k < n; k++)
        {
            var vkp = v[k, p];
            var vkq = v[k, q];

            v[k, p] = c * vkp - s * vkq;
            v[k, q] = s * vkp + c * vkq;
        }
    }

    private static double Soma(double[,] a, int n)
    {
        var soma = 0.0;

        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                if (i != j) soma += a[i, j] * a[i, j];

        return soma;
    }

    private static double[] Ordenar(double[,] a, int n)
    {
        var d = new double[n];
        for (var i = 0; i < n; i++) d[i] = a[i, i];

        Array.Sort(d);
        Array.Reverse(d);
        return d;
    }

    public static double[] Autovalores(Matriz matriz) => Diagonalizar(matriz).Autovalores;

    /// <summary>
    /// Confere que a soma dos quadrados de fora da diagonal SO CAI, rotacao a
    /// rotacao.
    ///
    /// Essa e a quantidade que garante a convergencia, e ela e medivel. Se ela
    /// subisse em alguma rotacao, a garantia nao existiria.
    /// </summary>
    public static bool SoCai(List<double> trilha)
    {
        ArgumentNullException.ThrowIfNull(trilha);

        for (var i = 1; i < trilha.Count; i++)
            if (trilha[i] > trilha[i - 1] + 1e-18) return false;

        return true;
    }
}
