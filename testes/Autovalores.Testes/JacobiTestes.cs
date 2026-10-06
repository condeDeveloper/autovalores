using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

public class JacobiTestes
{
    /// <summary>
    /// A QUANTIDADE QUE SO CAI: a soma dos quadrados de fora da diagonal nunca
    /// sobe, em nenhuma rotacao, em nenhuma matriz.
    ///
    /// E ela que garante a convergencia, e ela e medivel passo a passo: nao e
    /// argumento, e numero.
    /// </summary>
    [Fact]
    public void ASomaDeForaDaDiagonalSoCai()
    {
        foreach (var (nome, matriz) in Casos.Simetricas())
        {
            var r = Jacobi.Diagonalizar(matriz);

            Assert.True(Jacobi.SoCai(r.ForaDaDiagonal), $"{nome} subiu em alguma rotacao");
            Assert.True(r.Convergiu, nome);
        }
    }

    /// <summary>E ela cai de verdade: do comeco ate o fim, varias ordens de grandeza.</summary>
    [Fact]
    public void ElaCaiVariasOrdensDeGrandeza()
    {
        var trilha = Jacobi.Diagonalizar(Casos.Simetrica4()).ForaDaDiagonal;

        Assert.True(trilha.Count > 5);
        Assert.True(trilha[0] > 1);
        Assert.True(trilha[^1] < trilha[0] / 1_000_000);
    }

    /// <summary>Os autovalores batem com a formula exata, em todas as simetricas.</summary>
    [Fact]
    public void OsAutovaloresBatemComOExato()
    {
        foreach (var (nome, matriz) in Casos.Simetricas())
        {
            if (matriz.Ordem > 3) continue;

            var exatos = Exata.De(matriz)!;
            Assert.True(Residuo.Distancia(exatos, Jacobi.Autovalores(matriz)) < 1e-9, nome);
        }
    }

    /// <summary>
    /// E as duas identidades confirmam, inclusive na de ordem quatro, onde nao
    /// ha formula fechada para comparar.
    /// </summary>
    [Fact]
    public void AsIdentidadesConfirmamAteNaOrdemQuatro()
    {
        var quatro = Casos.Simetrica4();
        var autovalores = Jacobi.Autovalores(quatro);

        Assert.Equal(4, autovalores.Length);
        Assert.True(Residuo.ErroDoTraco(quatro, autovalores) < 1e-9);
        Assert.True(Residuo.ErroDoDeterminante(quatro, autovalores) < 1e-8);
    }

    /// <summary>
    /// Os AUTOVETORES tambem saem, e eles passam no teste da definicao: A vezes
    /// v e L vezes v.
    /// </summary>
    [Fact]
    public void OsAutovetoresPassamNoTesteDaDefinicao()
    {
        foreach (var (nome, matriz) in Casos.Simetricas())
        {
            var r = Jacobi.Diagonalizar(matriz);

            for (var k = 0; k < matriz.Ordem; k++)
            {
                var v = new double[matriz.Ordem];
                for (var i = 0; i < matriz.Ordem; i++) v[i] = r.Autovetores[i, k];

                var autovalor = Vetor.Interno(v, matriz.Vezes(v)) / Vetor.Interno(v, v);
                Assert.True(Residuo.De(matriz, autovalor, v) < 1e-9, $"{nome}, autovetor {k}");
            }
        }
    }

    /// <summary>
    /// E os autovetores sao ORTOGONAIS entre si, que e a propriedade que so
    /// matriz simetrica tem.
    /// </summary>
    [Fact]
    public void OsAutovetoresSaoOrtogonais()
    {
        var r = Jacobi.Diagonalizar(Casos.Simetrica4());
        var n = 4;

        for (var a = 0; a < n; a++)
            for (var b = a + 1; b < n; b++)
            {
                var va = new double[n];
                var vb = new double[n];

                for (var i = 0; i < n; i++) { va[i] = r.Autovetores[i, a]; vb[i] = r.Autovetores[i, b]; }

                Assert.True(Math.Abs(Vetor.Interno(va, vb)) < 1e-10);
            }
    }

    /// <summary>A matriz ja diagonal nao precisa de rotacao nenhuma.</summary>
    [Fact]
    public void ADiagonalNaoPrecisaDeRotacao()
    {
        var r = Jacobi.Diagonalizar(Matriz.Diagonal(5, 3, 1));

        Assert.Equal(0, r.Rotacoes);
        Assert.True(r.Convergiu);
        Assert.Equal(new[] { 5.0, 3, 1 }, r.Autovalores);
    }

    /// <summary>
    /// Jacobi so vale para matriz SIMETRICA, e ele recusa a outra em vez de
    /// devolver numero sem significado.
    /// </summary>
    [Fact]
    public void JacobiRecusaMatrizNaoSimetrica()
    {
        Assert.Throws<ArgumentException>(() => Jacobi.Diagonalizar(Casos.Jordan()));
        Assert.Throws<ArgumentException>(() => Jacobi.Diagonalizar(Casos.Rotacao()));
    }

    [Fact]
    public void ARotacaoEscolheOMaiorElementoDeForaDaDiagonal()
    {
        // uma matriz em que o maior esta no canto, e nao no primeiro par
        var matriz = Matriz.De([1, 0.001, 5], [0.001, 2, 0.001], [5, 0.001, 3]);
        var r = Jacobi.Diagonalizar(matriz);

        Assert.True(r.Convergiu);
        Assert.True(r.ForaDaDiagonal[1] < r.ForaDaDiagonal[0] / 10,
                    "a primeira rotacao nao atacou o maior elemento");
    }

    [Fact]
    public void ATrilhaSoCaiEhExigenteDeVerdade()
    {
        Assert.True(Jacobi.SoCai([10, 5, 1, 0.1]));
        Assert.False(Jacobi.SoCai([10, 5, 6, 0.1]));
        Assert.True(Jacobi.SoCai([]));
    }
}
