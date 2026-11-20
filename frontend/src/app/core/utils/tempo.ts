/**
 * Formatta un istante passato come tempo relativo ("2 ore fa", "ieri"), nello
 * stile delle righe di richiesta e attività del design. Sotto il minuto
 * restituisce "adesso" invece di "0 minuti fa".
 */
export function tempoRelativo(data: string | Date): string {
  const istante = typeof data === 'string' ? new Date(data) : data;
  const secondi = Math.max(0, (Date.now() - istante.getTime()) / 1000);

  if (secondi < 60) {
    return 'adesso';
  }
  if (secondi < 3600) {
    const minuti = Math.floor(secondi / 60);
    return `${minuti} ${minuti === 1 ? 'minuto' : 'minuti'} fa`;
  }
  if (secondi < 86400) {
    const ore = Math.floor(secondi / 3600);
    return `${ore} ${ore === 1 ? 'ora' : 'ore'} fa`;
  }
  if (secondi < 172800) {
    return 'ieri';
  }

  const giorni = Math.floor(secondi / 86400);
  if (giorni < 7) {
    return `${giorni} giorni fa`;
  }

  return istante.toLocaleDateString('it-IT', { day: 'numeric', month: 'long' });
}
