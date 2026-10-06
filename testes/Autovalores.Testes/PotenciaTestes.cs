using Conde.Autovalores;
using Xunit;

namespace Conde.Autovalores.Testes;

public class PotenciaTestes
{
    /// <summary>
    /// Nas matrizes em que o dominante e unico em modulo, o metodo acha ele, e o
    /// RESIDUO confirma: A vezes v menos L vezes v e quase zero.
    /// </summary>
    [Fact]
    public void OMetodoAchaODominanteQuandoEleEhUnico()
    {
        foreach (var (nome, matriz) in new[]
                 {
                     ("diagonal", Casos.Diagonal()),
                     ("simetrica", Casos.Simetrica()),
                     ("linhas iguais", Casos.LinhasIguais()),
                 })
        {
            var exatos = Exata.De(matriz)!;
            var dominante = exatos.OrderByDescending(Math.Abs).First();
            var r = Potencia.Dominante(matriz);

            Assert.True(r.Convergiu, nome);
            Assert.Equal(dominante, r.Autovalor, 5);
            Assert.True(Residuo.De(matriz, r.Autovalor, r.Autovetor) < 1e-5, nome);
        }
    }

    /// <summary>
    /// O ACHADO: no ESPELHO ele diz que convergiu, em dois passos, com residuo
    /// 0,38.
    ///
    /// O criterio de parada e "o quociente de Rayleigh parou de mudar", e no
    /// espelho ele nunca muda, porque trocar as duas coordenadas de lugar nao
    /// muda o quociente. "Parou de mudar" nao e "esta certo", e so o residuo
    /// sabe a diferenca.
    /// </summary>
    [Fact]
    public void NoEspelhoEleDizQueConvergiuComResiduoEnorme()
    {
        var espelho = Casos.Espelho();
        var r = Potencia.Dominante(espelho);

        Assert.True(r.Convergiu);
        Assert.True(Residuo.De(espelho, r.Autovalor, r.Autovetor) > 0.1,
                    $"o residuo foi {Residuo.De(espelho, r.Autovalor, r.Autovetor)}");
        Assert.NotEqual(1.0, r.Autovalor, 2);
    }

    /// <summary>
    /// E a razao entre o segundo e o primeiro autovalor em modulo e UM no
    /// espelho: as duas parcelas do vetor nunca se separam.
    /// </summary>
    [Fact]
    public void ARazaoDoEspelhoEhUm()
    {
        Assert.Equal(1, Potencia.Razao(Exata.De(Casos.Espelho())!), 12);
        Assert.Equal(0.6, Potencia.Razao(Exata.De(Casos.Diagonal())!), 12);
        Assert.Equal(0.5, Potencia.Razao(Exata.De(Casos.LinhasIguais())!), 12);
    }

    /// <summary>
    /// A ARMADILHA DO VETOR DE PARTIDA: o vetor de todos iguais a um e
    /// autovetor do autovalor que NAO e o dominante, e o metodo fica parado nele
    /// dizendo que convergiu.
    ///
    /// Comecar com todos iguais a um e o palpite mais natural do mundo.
    /// </summary>
    [Fact]
    public void OVetorDeTodosIguaisPodeSerUmaArmadilha()
    {
        var matriz = Casos.LinhasIguais();

        var comUns = Potencia.Dominante(matriz, [1, 1]);
        var comPadrao = Potencia.Dominante(matriz);

        Assert.True(comUns.Convergiu);
        Assert.Equal(2, comUns.Autovalor, 9);

        Assert.Equal(4, comPadrao.Autovalor, 5);
    }

    /// <summary>
    /// A razao dita a velocidade: quanto mais perto de um, mais passos. Com
    /// 0,999 sao milhares.
    /// </summary>
    [Fact]
    public void ARazaoDitaOsPassos()
    {
        var rapida = Potencia.Dominante(Casos.Diagonal()).Passos;
        var lenta = Potencia.Dominante(Casos.Proximos(), limite: 20_000).Passos;

        Assert.True(lenta > rapida * 100, $"rapida {rapida}, lenta {lenta}");
    }

    /// <summary>
    /// A POTENCIA INVERSA acha QUALQUER autovalor, dado um palpite perto dele.
    /// </summary>
    [Fact]
    public void APotenciaInversaAchaQualquerAutovalor()
    {
        var simetrica = Casos.Simetrica();
        var exatos = Exata.De(simetrica)!;

        foreach (var alvo in exatos)
        {
            var r = Potencia.Inversa(simetrica, alvo + 0.3);

            Assert.True(r.Convergiu);
            Assert.Equal(alvo, r.Autovalor, 5);
            Assert.True(Residuo.De(simetrica, r.Autovalor, r.Autovetor) < 1e-5);
        }
    }

    [Fact]
    public void OPalpiteExatoNaoQuebraAInversa()
    {
        var r = Potencia.Inversa(Casos.Diagonal(), 3);
        Assert.Equal(3, r.Autovalor, 6);
    }

    [Fact]
    public void OVetorDePartidaPadraoNaoTemCoordenadasIguais()
    {
        var v = Potencia.Padrao(4);

        Assert.Equal(4, v.Length);
        Assert.Equal(4, v.Distinct().Count());
        Assert.True(Vetor.Norma(v) > 0);
    }

    [Fact]
    public void OSistemaEhResolvidoDireito()
    {
        var a = Matriz.De([2, 1], [1, 3]);
        var x = Potencia.Resolver(a, [5, 10])!;

        var volta = a.Vezes(x);
        Assert.Equal(5, volta[0], 9);
        Assert.Equal(10, volta[1], 9);
    }

    [Fact]
    public void SistemaSingularDevolveNulo()
    {
        Assert.Null(Potencia.Resolver(Matriz.De([1, 2], [2, 4]), [1, 2]));
    }

    [Fact]
    public void ARazaoDeUmAutovalorSoEhZero()
    {
        Assert.Equal(0, Potencia.Razao([5]), 12);
        Assert.Equal(0, Potencia.Razao([0, 0]), 12);
    }
}
