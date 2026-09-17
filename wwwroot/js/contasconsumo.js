(() => {
  "use strict";

  const iniciar = () => {
    const leituraAnterior = document.querySelector("[name='LeituraAnterior']");
    const leituraAtual = document.querySelector("[name='LeituraAtual']");
    const consumo = document.querySelector("[name='Consumo']");
    if (!leituraAnterior || !leituraAtual || !consumo || leituraAnterior.dataset.consumoIniciado) return;
    leituraAnterior.dataset.consumoIniciado = "true";

    const calcularConsumo = () => {
      const a = Util.parseNumero(leituraAnterior.value);
      const b = Util.parseNumero(leituraAtual.value);
      if (!Number.isNaN(a) && !Number.isNaN(b)) {
        consumo.value = Util.formatarDecimal(b - a, 3);
      }
    };

    leituraAnterior.addEventListener("input", calcularConsumo);
    leituraAtual.addEventListener("input", calcularConsumo);
  };

  document.addEventListener("app:navigated", iniciar);
  iniciar();
})();
