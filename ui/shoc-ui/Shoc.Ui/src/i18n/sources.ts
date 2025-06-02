import landingEn from "./messages/en/landing.json";
import globalEn from "./messages/en/global.json";
import authEn from "./messages/en/auth.json";
import componentsEn from "./messages/en/components.json";
import errorsEn from "./messages/en/errors.json";
import workspacesEn from "./messages/en/workspaces.json";
import clustersEn from "./messages/en/clusters.json";
import secretsEn from "./messages/en/secrets.json";
import templatesEn from "./messages/en/templates.json";
import jobsEn from "./messages/en/jobs.json";
import localeConfig from "./locale-config";

export const localeMessages = {
   
    en: {
        ...landingEn,
        ...globalEn,
        ...authEn,
        ...componentsEn,
        ...errorsEn,
        ...workspacesEn,
        ...clustersEn,
        ...secretsEn,
        ...templatesEn,
        ...jobsEn
    }
};

export type IntlMessageId = keyof typeof localeMessages.en

declare global {
    namespace FormatjsIntl {
        interface Message {
            ids: IntlMessageId
        }
    }
}


export function getLocaleSource(locale?: string) {

    if(!localeConfig.availableLocales.some(item => item === locale)){
        locale = localeConfig.defaultLocale;
    }

    locale = locale || localeConfig.defaultLocale;

    return {
        locale,
        defaultLocale: localeConfig.defaultLocale,
        messages: Object.fromEntries(Object.entries(localeMessages))[locale]
    }
}
