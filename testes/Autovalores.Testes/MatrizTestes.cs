using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

public class MatrizTestes
{
    [Fact]
    public void AMultiplicacaoEhOQueDiz()
    {
        var a = Matriz.De([1, 2], [3, 4]);
        var b = Matriz.De([5, 6], [7, 8]);
        var c = a.Vezes(b);

        Assert.Equal(19, c[0, 0], 12);
        Assert.Equal(22, c[0, 1], 12);
        Assert.Equal(43, c[1, 0], 12);
        Assert.Equal(50, c[1, 1], 12);
    }

    [Fact]
    public void AIdentidadeNaoMudaNada()
    {
        var a = Casos.Simetrica();
        var produto = a.Vezes(Matriz.Identidade(3));

        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                Assert.Equal(a[i, j], produto[i, j], 12);
    }

    [Fact]
    public void AMatrizVezesVetorEhOQueDiz()
    {
        var a = Matriz.De([1, 2], [3, 4]);
        Assert.Equal(new[] { 5.0, 11.0 }, a.Vezes(new[] { 1.0, 2.0 }));
    }

    /// <summary>
    /// O determinante de uma triangular e o produto da diagonal, e o de uma
    /// matriz com duas linhas iguais e zero. Dois casos em que a resposta e
    /// conhecida sem conta nenhuma.
    /// </summary>
    [Fact]
    public void ODeterminanteBateNosCasosConhecidos()
    {
        Assert.Equal(15, Matriz.Diagonal(5, 3, 1).Determinante(), 10);
        Assert.Equal(1, Matriz.Identidade(4).Determinante(), 10);
        Assert.Equal(0, Matriz.De([1, 2], [2, 4]).Determinante(), 10);
        Assert.Equal(-2, Matriz.De([1, 2], [3, 4]).Determinante(), 10);
    }

    [Fact]
    public void OTracoEhASomaDaDiagonal()
    {
        Assert.Equal(9, Matriz.Diagonal(5, 3, 1).Traco(), 12);
        Assert.Equal(9, Casos.Simetrica().Traco(), 12);
        Assert.Equal(0, Casos.Rotacao().Traco(), 12);
    }

    /// <summary>
    /// O determinante do produto e o produto dos determinantes, que e uma
    /// identidade e nao uma aproximacao.
    /// </summary>
    [Fact]
    public void ODeterminanteDoProdutoEhOProdutoDosDeterminantes()
    {
        var a = Casos.Simetrica();
        var b = Matriz.De([1, 2, 0], [0, 1, 3], [2, 0, 1]);

        Assert.Equal(a.Determinante() * b.Determinante(), a.Vezes(b).Determinante(), 9);
    }

    [Fact]
    public void ATranspostaTrocaLinhaComColuna()
    {
        var a = Matriz.De([1, 2], [3, 4]);
        var t = a.Transposta();

        Assert.Equal(a[0, 1], t[1, 0], 12);
        Assert.Equal(a[1, 0], t[0, 1], 12);
        Assert.True(Casos.Simetrica().Simetrica());
        Assert.False(Casos.Jordan().Simetrica());
    }

    [Fact]
    public void ODeslocamentoSoMexeNaDiagonal()
    {
        var a = Casos.Simetrica();
        var b = a.MenosDiagonal(2);

        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                Assert.Equal(a[i, j] - (i == j ? 2 : 0), b[i, j], 12);
    }

    [Fact]
    public void AForaDaDiagonalEhZeroNaDiagonal()
    {
        Assert.Equal(0, Matriz.Diagonal(5, 3, 1).ForaDaDiagonal(), 12);
        Assert.True(Casos.Simetrica().ForaDaDiagonal() > 0);
    }

    [Fact]
    public void MatrizNaoQuadradaEhRecusada()
    {
        Assert.Throws<ArgumentException>(() => new Matriz(new double[2, 3]));
    }

    [Fact]
    public void AsContasDeVetorSaoOQueDizem()
    {
        Assert.Equal(5, Vetor.Norma(new[] { 3.0, 4.0 }), 12);
        Assert.Equal(11, Vetor.Interno(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 }), 12);
        Assert.Equal(1, Vetor.Norma(Vetor.Normalizar(new[] { 3.0, 4.0 })), 12);
        Assert.Equal(new[] { -2.0, -2.0 }, Vetor.Menos(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 }));
        Assert.Equal(new[] { 2.0, 4.0 }, Vetor.Vezes(2, new[] { 1.0, 2.0 }));
    }

    [Fact]
    public void NormalizarOVetorZeroNaoQuebra()
    {
        Assert.Equal(new[] { 0.0, 0.0 }, Vetor.Normalizar(new[] { 0.0, 0.0 }));
    }
}
