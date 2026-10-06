# autovalores

Autovalores do zero em C# e .NET 8: método da potência, Jacobi, o algoritmo QR e
o deslocamento de Wilkinson. O juiz é a **fórmula fechada** em ordem dois e três,
mais duas identidades que valem sempre.

```
$ dotnet medidor.dll potencia

matriz             dominante      achado    passos     razao     residuo   convergiu
diagonal             5.00000     5.00000        28    0.6000    8.2E-007         sim
simetrica            4.87939     4.87939        19    0.5437    7.3E-007         sim
proximos             1.00101     1.00101     7,332    0.9990    7.1E-007         sim
espelho              1.00000     0.92308         2    1.0000    3.8E-001         sim
Jordan               1.00000     1.00005    20,000    1.0000    2.5E-009         NAO
```

Olhe a linha do **espelho**. Ele diz que convergiu, em dois passos, com resíduo
`0,38`.

O critério de parada é "o quociente de Rayleigh parou de mudar", e no espelho ele
nunca muda, porque trocar as duas coordenadas de lugar não muda o quociente.
**"Parou de mudar" não é "está certo"**, e só o resíduo sabe a diferença: ele mede
`A·v − λ·v`, que é a definição, e não depende de confiar em quem calculou.

## O método da potência só acha o dominante, e nem sempre

A coluna da razão é o segundo maior autovalor em módulo dividido pelo maior, e é
ela que dita a velocidade. Com `0,6`, vinte e oito passos; com `0,999`, sete mil;
com **um**, nunca.

No espelho os autovalores são `1` e `−1`, de mesmo módulo. As duas parcelas do
vetor nunca se separam e ele oscila entre duas direções para sempre. A matriz não
tem defeito nenhum e o método não funciona nela: isso não é caso de borda, é a
condição de uso, escrita em número.

E tem a armadilha do vetor de partida:

```
    a matriz tem autovalores 4 e 2, e as linhas somam dois
    comecando em (1, 1):   achou 2.00000 em 2 passo(s), dizendo que convergiu: True
    comecando no padrao:   achou 4.00000 em 24 passos
```

O vetor de todos iguais a um **já é autovetor**, e do autovalor que não é o
dominante. Começar com ele é o palpite mais natural do mundo.

## Jacobi: a quantidade que só cai

```
$ dotnet medidor.dll jacobi

  a soma de fora da diagonal, rotacao a rotacao, na simetrica de ordem 4:
    rotacao  0      2.8000E+001
    rotacao  1      2.0000E+001
    rotacao  2      4.2389E+000
    rotacao  5      4.0012E-001
    rotacao  8      2.7045E-003
    rotacao 11      5.4234E-006
```

Cada rotação zera **um** elemento de fora da diagonal e estraga um pouco os zeros
anteriores. Mesmo assim a soma dos quadrados de fora da diagonal **cai em todas
as rotações**, sem exceção, e é isso que garante a convergência.

Ela não é argumento: é número, e dá para olhar cair.

## A fatoração QR: a mesma conta em outra ordem

```
$ dotnet medidor.dll qr

matriz                   classico: Q nao ortogonal    modificado         razao
simetrica                                2.22E-016     2.22E-016           1.0
quase dependente                         7.07E-001     7.07E-009 100,000,000.0
```

As duas versões do Gram-Schmidt fazem as **mesmas contas em outra ordem**. A
clássica projeta na coluna original; a modificada projeta no que sobrou dela
depois das projeções anteriores.

Na matriz quase dependente, isso é a diferença entre um `Q` quase ortogonal e um
que não é ortogonal nem de longe. E o detalhe que engana:

```
  matriz                      classico    modificado
  quase dependente           0.00E+000     0.00E+000
```

`Q·R` devolve a matriz nas duas, então a conferência óbvia passa. O que quebra é a
**ortogonalidade de Q**, e só quem olha para ela vê.

## O deslocamento: uma linha, ordens de grandeza

```
$ dotnet medidor.dll deslocamento

matriz              sem deslocamento       com       razao   erro contra o exato
simetrica                         39         7         5.6             8.88E-016
proximos                      18,101         2     9,050.5             2.53E-014
espelho                       50,000         2    25,000.0             2.22E-016
```

O laço QR sem deslocamento converge pela razão entre autovalores vizinhos,
elevada ao número de voltas. Com dois autovalores parecidos isso é quase um, e ele
gira dezoito mil vezes.

Com o deslocamento, a velocidade passa a depender das **distâncias** até o
deslocamento escolhido, e escolhendo ele perto de um autovalor a razão vira quase
zero.

## O condicionamento é da matriz, não do método

```
$ dotnet medidor.dll condicionamento

matriz             simetrica   cutucada de   autovalores andam       razao
simetrica                sim        1E-010           7.12E-011           1
espelho                  sim        1E-010           5.00E-011           1
Jordan                   nao        1E-010           1.00E-005     100,000
```

Nas matrizes simétricas, uma perturbação de tamanho `e` move os autovalores no
máximo `e`. Não há método ruim que estrague isso nem método bom que melhore.

No bloco de Jordan a perturbação é amplificada pela **raiz**:

```
      cutucada         andou    raiz da cutucada
        1E-004     1.00E-002           1.00E-002
        1E-008     1.00E-004           1.00E-004
        1E-012     1.00E-006           1.00E-006
```

As duas últimas colunas são o mesmo número. Autovalor repetido sem base de
autovetores faz o **problema** ser mal condicionado, e nenhum método conserta,
porque não é defeito de método. É por isso que um erro de arredondamento de
`1e-16` pode virar um erro de `1e-8` na resposta.

## O que as medidas me corrigiram

**Minha deflação acontecia cedo demais, e o laço dizia que tinha convergido.** O
critério olhava só o elemento vizinho abaixo da diagonal, e na simétrica de ordem
três a última linha ainda tinha peso na primeira coluna. O autovalor guardado não
era autovalor de nada: erro de `5,3E-001` contra a fórmula exata, com o método
reportando sucesso. A linha **inteira** abaixo da diagonal precisa estar
desprezível.

**A fórmula fechada não devolve os números cravados, nem numa matriz diagonal.**
Em ordem três ela passa por Cardano, com raiz, arco cosseno e cosseno, e o `1` de
`diag(5, 3, 1)` volta como `0,99999999999999956`. A fórmula é exata em matemática
e não é exata em ponto flutuante. Pedir igualdade bit a bit do juiz seria pedir
uma coisa que ele nunca prometeu.

**O caso que eu tinha montado para a armadilha do vetor de partida não tinha
autovalor real nenhum.** Era uma circulante de ordem três com soma de linhas
constante, e os autovalores dela são complexos, então o método nem chegava perto
de cair na armadilha. Trocar por uma simétrica de ordem dois com autovalores 4 e 2
fez a armadilha funcionar: o vetor de uns é autovetor do **2**.

**`Q·R` devolver a matriz não prova que `Q` é ortogonal.** Essa era a conferência
que eu ia escrever para a fatoração, e ela passa nas duas versões, inclusive na
que produz um `Q` com desvio de `0,7`. A conferência que separa as duas é outra.

## Por que o juiz é a fórmula fechada

Método iterativo de autovalor erra de um jeito silencioso: ele devolve um número
que mudou pouco na última iteração, e "mudou pouco" não é "está certo". Uma matriz
quase singular, ou com dois autovalores quase iguais, faz a iteração parecer
convergida longe da resposta.

E duas conferências saem de graça e valem sempre: a soma dos autovalores é o
**traço** e o produto é o **determinante**. Elas não dizem quais autovalores são,
e dizem na hora se o conjunto está errado.

Nenhuma medida aqui usa relógio. Todas são contagem de iterações, resíduos ou
identidades conferidas, então a saída é a mesma no Linux, no Windows e no macOS, e
a integração contínua roda nos três.

## As peças

| arquivo | o que faz |
| --- | --- |
| `Matriz.cs` | a matriz, o traço, o determinante e as contas de vetor |
| `Exata.cs` | Bhaskara e Cardano, que são o juiz |
| `Potencia.cs` | o método da potência, o quociente de Rayleigh e a potência inversa |
| `Jacobi.cs` | as rotações e a quantidade que só cai |
| `Qr.cs` | Gram-Schmidt clássico e modificado, e o laço QR |
| `Deslocamento.cs` | o deslocamento de Wilkinson e a deflação |
| `Residuo.cs` | a definição conferida, as identidades e o condicionamento |
| `Casos.cs` | as matrizes que quebram cada método |

## Como rodar

```
dotnet test testes/Autovalores.Testes/Autovalores.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

As medidas aceitam `juiz`, `potencia`, `jacobi`, `qr`, `deslocamento`,
`condicionamento` e `tudo`.

## Licença

MIT.
