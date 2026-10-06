using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

public class DeslocamentoTestes
{
    /// <summary>
    /// O ACHADO: o laco com deslocamento acha os mesmos autovalores em ordens de
    /// grandeza menos voltas.
    /// </summary>
    [Fact]
    public void ODeslocamentoEconomizaOrdensDeGrandeza()
    {
        foreach (var (nome, matriz) in new[] { ("proximos", Casos.Proximos()), ("espelho", Casos.Espelho()) })
        {
            var (sem, com) = Deslocamento.Comparar(matriz);

            Assert.True(sem > com * 1000, $"{nome}: sem {sem}, com {com}");
            Assert.True(com < 20, $"{nome}: o com deslocamento gastou {com}");
        }
    }

    /// <summary>E os autovalores achados batem com os exatos, em todas as matrizes.</summary>
    [Fact]
    public void OsAutovaloresBatemComOExato()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var exatos = Exata.De(matriz)!;
            var achados = Deslocamento.Iterar(matriz).Autovalores;

            Assert.Equal(exatos.Length, achados.Length);
            Assert.True(Residuo.Distancia(exatos, achados) < 1e-8,
                        $"{nome}: {Residuo.Distancia(exatos, achados)}");
        }
    }

    /// <summary>
    /// A DEFLACAO cedo demais foi um erro de verdade: olhando so o elemento
    /// vizinho abaixo da diagonal, a simetrica de ordem tres deflacionava com
    /// peso ainda na primeira coluna, e o laco devolvia um autovalor errado por
    /// 5,3E-001 dizendo que tinha convergido.
    ///
    /// A linha INTEIRA abaixo da diagonal precisa estar desprezivel.
    /// </summary>
    [Fact]
    public void ADeflacaoSoAconteceComALinhaInteiraDesprezivel()
    {
        var simetrica = Casos.Simetrica();
        var r = Deslocamento.Iterar(simetrica);

        Assert.True(r.Convergiu);
        Assert.True(Residuo.Distancia(Exata.De(simetrica)!, r.Autovalores) < 1e-10);
        Assert.True(Residuo.ErroDoTraco(simetrica, r.Autovalores) < 1e-9);
        Assert.True(Residuo.ErroDoDeterminante(simetrica, r.Autovalores) < 1e-8);
    }

    /// <summary>
    /// O deslocamento de WILKINSON pega o autovalor do bloco dois por dois do
    /// canto que esta mais perto do ultimo elemento da diagonal.
    /// </summary>
    [Fact]
    public void OWilkinsonPegaOMaisPertoDoCanto()
    {
        var matriz = Matriz.De([5, 0], [0, 1]);

        // o bloco e a propria matriz, com autovalores 5 e 1; o canto e 1
        Assert.Equal(1, Deslocamento.DeWilkinson(matriz, 1), 9);
    }

    /// <summary>
    /// E ele enxerga o caso em que o canto e ZERO, que e onde pegar simplesmente
    /// o ultimo elemento da diagonal nao desloca nada.
    /// </summary>
    [Fact]
    public void OWilkinsonEnxergaOCantoZero()
    {
        var espelho = Casos.Espelho();

        Assert.Equal(0, espelho[1, 1], 12);
        Assert.NotEqual(0, Deslocamento.DeWilkinson(espelho, 1), 6);
    }

    [Fact]
    public void ComOrdemUmONaoHaDeslocamentoAFazer()
    {
        Assert.Equal(7, Deslocamento.DeWilkinson(Matriz.Diagonal(7), 0), 12);
    }

    /// <summary>
    /// Quando o bloco do canto tem autovalores complexos, o deslocamento cai no
    /// ultimo elemento da diagonal em vez de devolver um numero sem sentido.
    /// </summary>
    [Fact]
    public void ComBlocoComplexoEleUsaOCanto()
    {
        var rotacao = Casos.Rotacao();
        Assert.Equal(rotacao[1, 1], Deslocamento.DeWilkinson(rotacao, 1), 12);
    }

    [Fact]
    public void ADiagonalJaVemPronta()
    {
        var r = Deslocamento.Iterar(Matriz.Diagonal(5, 3, 1));

        Assert.True(r.Convergiu);
        Assert.Equal(new[] { 5.0, 3, 1 }, r.Autovalores);
        Assert.True(r.Iteracoes <= 3);
    }

    /// <summary>
    /// As duas identidades valem para o que o laco devolve, em todas as
    /// matrizes com autovalores reais.
    /// </summary>
    [Fact]
    public void AsIdentidadesValemParaOQueOLacoDevolve()
    {
        foreach (var (nome, matriz) in Casos.ComAutovaloresReais())
        {
            var achados = Deslocamento.Iterar(matriz).Autovalores;

            Assert.True(Residuo.ErroDoTraco(matriz, achados) < 1e-8, nome);
            Assert.True(Residuo.ErroDoDeterminante(matriz, achados) < 1e-7, nome);
        }
    }

    [Fact]
    public void OLacoDevolveUmAutovalorPorOrdem()
    {
        foreach (var (_, matriz) in Casos.Todas())
            Assert.Equal(matriz.Ordem, Deslocamento.Iterar(matriz, 1e-10, 500).Autovalores.Length);
    }
}
