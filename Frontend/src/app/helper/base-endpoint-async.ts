import {Observable} from 'rxjs';

export interface BaseEndpointAsync<Trequest = void, Tresponse = void> {
  handleAsync(request:Trequest): Observable<Tresponse>;
}
