// https://github.com/bugsnag/bugsnag-js/blob/master/packages/plugin-angular/src/index.ts

import { ErrorHandler, Injectable } from "@angular/core";
import { ApplicationInsightsService } from "../business/application-insights.service";

@Injectable({
    providedIn: 'root'
  })
export class ApplicationInsightsErrorHandler extends ErrorHandler {
  
  constructor(private appInsights: ApplicationInsightsService) {
    super();
  }

  public handleError(error: any): void {
    this.appInsights.trackError(error);
    ErrorHandler.prototype.handleError.call(this, error);
  }
}