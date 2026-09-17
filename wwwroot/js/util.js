(() => {
  "use strict";

  const soDigitos = (valor) => String(valor ?? "").replace(/\D/g, "");

  const parseNumero = (valor) => {
    const texto = String(valor ?? "").trim();
    if (!texto) return NaN;
    if (texto.includes(",")) return Number(texto.replace(/\./g, "").replace(",", "."));
    return Number(texto);
  };

  const formatarMoeda = (valor) =>
    Number(valor).toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

  const formatarDecimal = (valor, casas = 2) =>
    Number(valor).toFixed(casas).replace(".", ",");

  const aplicarMascara = (entrada, formatador) => {
    entrada.addEventListener("input", () => {
      const posicao = entrada.selectionStart;
      const anterior = entrada.value;
      entrada.value = formatador(entrada.value);
      const delta = entrada.value.length - anterior.length;
      entrada.setSelectionRange(Math.max(0, (posicao ?? 0) + delta), Math.max(0, (posicao ?? 0) + delta));
    });
  };

  const mascaras = {
    cpf: (v) => {
      const d = soDigitos(v).slice(0, 11);
      return d
        .replace(/(\d{3})(\d)/, "$1.$2")
        .replace(/(\d{3})(\d)/, "$1.$2")
        .replace(/(\d{3})(\d{1,2})$/, "$1-$2");
    },
    telefone: (v) => {
      const d = soDigitos(v).slice(0, 11);
      if (d.length <= 10) {
        return d.replace(/(\d{2})(\d)/, "($1) $2").replace(/(\d{4})(\d)/, "$1-$2");
      }
      return d.replace(/(\d{2})(\d)/, "($1) $2").replace(/(\d{5})(\d)/, "$1-$2");
    },
    cep: (v) => soDigitos(v).slice(0, 8).replace(/(\d{5})(\d)/, "$1-$2")
  };

  const iniciarMascaras = () => {
    document.querySelectorAll("[data-mascara]").forEach((el) => {
      if (el.dataset.mascaraIniciada) return;
      el.dataset.mascaraIniciada = "true";
      const tipo = el.getAttribute("data-mascara");
      if (tipo === "moeda") {
        el.addEventListener("blur", () => {
          const n = parseNumero(el.value);
          if (!Number.isNaN(n) && el.value !== "") {
            el.value = n.toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
          }
        });
        return;
      }
      if (mascaras[tipo]) aplicarMascara(el, mascaras[tipo]);
    });
  };

  window.Util = { soDigitos, parseNumero, formatarMoeda, formatarDecimal };
  document.addEventListener("app:navigated", iniciarMascaras);
  iniciarMascaras();
})();
