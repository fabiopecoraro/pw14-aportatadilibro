/**
 * Component library del design system.
 *
 * Punto di importazione unico per i componenti di presentazione condivisi:
 * le schermate importano da qui invece di raggiungere i singoli file, cosi' le
 * riorganizzazioni interne della cartella non si propagano ai feature module.
 */
export { Avatar, type AvatarVariante } from './avatar/avatar';
export { AuthPanel, type AuthPanelVariante } from './auth-panel/auth-panel';
export { BookCard, type BookCardVariante } from './book-card/book-card';
export { BookCover, type CopertinaDimensione } from './book-cover/book-cover';
export { ChartBarre, type BarraGrafico } from './chart-barre/chart-barre';
export { ChartLinea, type PuntoGrafico } from './chart-linea/chart-linea';
export { EmptyState } from './empty-state/empty-state';
export { PublicHeader } from './public-header/public-header';
export { SelettoreCategorie } from './selettore-categorie/selettore-categorie';
export { Skeleton, type SkeletonForma } from './skeleton/skeleton';
export { StatusBadge, type BadgeVariante, type StatoBadge } from './status-badge/status-badge';
