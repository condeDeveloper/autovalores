using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

public class QrTestes
{
    /// <summary>
    /// Q vezes R devolve a matriz, nas DUAS versoes. Essa e a conferencia obvia,
    /// e ela passa nas duas: e por isso que ela nao serve para separar uma da
    /// outra.
    /// </summary>
    [Fact]
    public void QVezesRDevolveAMatrizNasDuasVersoes()
    {
        foreach (var (nome, matriz) in Casos.Todas().Append(("quase dependente", Casos.QuaseDependente())))
        {
            Assert.True(Qr.ErroDaFatoracao(matriz, Qr.Fatorar(matriz)) < 1e-12, $"{nome} modificado");
            Assert.True(Qr.ErroDaFatoracao(matriz, Qr.FatorarClassico(matriz)) < 1e-12, $"{nome} classico");
        }
    }

    /// <summary>
    /// O ACHADO: na matriz quase dependente, o Q do Gram-Schmidt CLASSICO nao e
    /// ortogonal nem de longe, e o do modificado e.
    ///
    /// As duas versoes fazem as mesmas contas em outra ordem. A classica projeta
    /// na coluna original; a modificada projeta no que sobrou dela depois das
    /// projecoes anteriores.
    /// </summary>
    [Fact]
    public void OClassicoPerdeAOrtogonalidadeEOModificadoNao()
    {
        var matriz = Casos.QuaseDependente();

        var classico = Qr.DesvioDeOrtogonalidade(Qr.FatorarClassico(matriz).Q);
        var modificado = Qr.DesvioDeOrtogonalidade(Qr.Fatorar(matriz).Q);

        Assert.True(classico > 0.1, $"o classico deu {classico}");
        Assert.True(modificado < 1e-6, $"o modificado deu {modificado}");
        Assert.True(classico / modificado > 1e6);
    }

    /// <summary>
    /// E nas matrizes bem comportadas as duas empatam: a diferenca so aparece
    /// quando as colunas sao quase dependentes.
    /// </summary>
    [Fact]
    public void NasMatrizesBemComportadasAsDuasEmpatam()
    {
        foreach (var (nome, matriz) in Casos.Todas())
        {
            var classico = Qr.DesvioDeOrtogonalidade(Qr.FatorarClassico(matriz).Q);
            var modificado = Qr.DesvioDeOrtogonalidade(Qr.Fatorar(matriz).Q);

            Assert.True(Math.Abs(classico - modificado) < 1e-12, nome);
        }
    }

    /// <summary>R e triangular superior, que e metade da definicao da fatoracao.</summary>
    [Fact]
    public void REhTriangularSuperior()
    {
        foreach (var (nome, matriz) in Casos.Todas())
        {
            var (_, r) = Qr.Fatorar(matriz);

            for (var i = 1; i < matriz.Ordem; i++)
                for (var j = 0; j < i; j++)
                    Assert.True(Math.Abs(r[i, j]) < 1e-14, $"{nome} em ({i}, {j})");
        }
    }

    /// <summary>
    /// O laco QR converge para uma triangular com os autovalores na diagonal, e
    /// eles batem com os exatos.
    /// </summary>
    [Fact]
    public void OLacoQrAchaOsAutovalores()
    {
        foreach (var (nome, matriz) in new[]
                 {
                     ("diagonal", Casos.Diagonal()),
                     ("simetrica", Casos.Simetrica()),
                     ("linhas iguais", Casos.LinhasIguais()),
                 })
        {
            var r = Qr.Iterar(matriz, 1e-10, 50_000);
            var exatos = Exata.De(matriz)!;

            Assert.True(r.Convergiu, nome);
            Assert.True(Residuo.Distancia(exatos, r.Autovalores) < 1e-7, nome);
        }
    }

    /// <summary>
    /// E ele NAO converge na rotacao: uma matriz sem autovalor real nao fica
    /// triangular por rotacao real nenhuma.
    /// </summary>
    [Fact]
    public void OLacoQrNaoConvergeNaRotacao()
    {
        Assert.False(Qr.Iterar(Casos.Rotacao(), 1e-10, 500).Convergiu);
    }

    /// <summary>
    /// A medida do quanto falta e a maior entrada ABAIXO da diagonal: ela e zero
    /// numa triangular e e o que o laco esta empurrando para zero.
    /// </summary>
    [Fact]
    public void AMedidaDoQueFaltaEhAbaixoDaDiagonal()
    {
        Assert.Equal(0, Qr.AbaixoDaDiagonal(Matriz.Diagonal(5, 3, 1)), 12);
        Assert.Equal(3, Qr.AbaixoDaDiagonal(Matriz.De([1, 2], [3, 4])), 12);

        var final = Qr.Iterar(Casos.Simetrica(), 1e-10, 50_000).Final;
        Assert.True(Qr.AbaixoDaDiagonal(final) < 1e-10);
    }

    /// <summary>
    /// O laco preserva o TRACO a cada volta: R vezes Q e a mesma matriz vista de
    /// outro angulo, e a soma dos autovalores nao muda.
    /// </summary>
    [Fact]
    public void OLacoPreservaOTraco()
    {
        var matriz = Casos.Simetrica();
        var traco = matriz.Traco();
        var atual = matriz;

        for (var i = 0; i < 20; i++)
        {
            var (q, r) = Qr.Fatorar(atual);
            atual = r.Vezes(q);

            Assert.Equal(traco, atual.Traco(), 9);
        }
    }

    [Fact]
    public void ADiagonalSaiOrdenada()
    {
        var d = Qr.Diagonal(Matriz.Diagonal(1, 5, 3));

        Assert.Equal(new[] { 5.0, 3, 1 }, d);
    }

    [Fact]
    public void AMatrizComColunaNulaNaoQuebraAFatoracao()
    {
        var matriz = Matriz.De([0, 1], [0, 1]);
        var (q, _) = Qr.Fatorar(matriz);

        Assert.Equal(2, q.Ordem);
        Assert.True(Qr.ErroDaFatoracao(matriz, Qr.Fatorar(matriz)) < 1e-12);
    }
}
