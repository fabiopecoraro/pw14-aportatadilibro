import { environment } from '../../../environments/environment';

/**
 * Converte un percorso pubblico relativo (es. "/covers/xxx.jpg" restituito
 * dal backend) in un URL assoluto verso l'API, da usare in `src` di `<img>`.
 */
export function resolveFileUrl(percorso: string | null | undefined): string | null {
  if (!percorso) {
    return null;
  }

  return `${environment.filesBaseUrl}${percorso}`;
}
