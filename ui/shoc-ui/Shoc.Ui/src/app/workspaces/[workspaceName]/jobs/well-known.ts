import { IntlMessageId } from "@/i18n/sources";

export const jobTaskTypes: { key: string, display: IntlMessageId }[] = [ 
    {
        key: 'function', 
        display: 'jobs.task.types.function'
    },
    {
        key: 'mpi', 
        display: 'jobs.task.types.mpi'
    }
]

export const jobTaskTypesMap = Object.assign({}, ...jobTaskTypes.map((entry) => ({[entry.key]: entry.display})));;
