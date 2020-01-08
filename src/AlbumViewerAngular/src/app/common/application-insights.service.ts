import { Injectable } from '@angular/core';
import { ApplicationInsights, IExceptionTelemetry } from '@microsoft/applicationinsights-web';

@Injectable({
    providedIn: 'root'
  })
export class ApplicationInsightsService {

    private appInsights = new ApplicationInsights({
        config: {
            instrumentationKey: '9d6565dc-c40e-4c0f-a09c-d21d8d533108',
            enableAutoRouteTracking: true,
            enableCorsCorrelation: true
        }	
    });

    constructor() {
        this.appInsights.loadAppInsights();
    }

    public setUserId(userId: string) {
        this.appInsights.setAuthenticatedUserContext(userId);
    }

    public clearUserId() {
        this.appInsights.clearAuthenticatedUserContext();
    }

    public trackError(error: Error) {
        let telemetry = {
            exception: error
        } as IExceptionTelemetry;
        this.appInsights.trackException(telemetry);
    }

    public logPageView(name?: string, uri?: string) {
        this.appInsights.trackPageView({ name, uri });
    }
}
