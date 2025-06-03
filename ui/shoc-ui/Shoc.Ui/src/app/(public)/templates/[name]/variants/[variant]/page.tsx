import { buttonVariants } from "@/components/ui/button";
import { ArrowLeftIcon } from "lucide-react";
import Link from "next/link";
import Typography from "@/components/vc/typography";
import getIntl from "@/i18n/get-intl";
import { Metadata } from "next";
import ErrorScreen from "@/components/error/error-screen";
import Markdown from 'react-markdown'
import CodeBlock from "@/components/vc/code-block";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion"
import { getVariant } from "../../../cached-template-actions";

type PageProps = {
    params: Promise<{ name: string, variant: string }>;
};

export const dynamic = 'force-dynamic';

export async function generateMetadata(props: { params: Promise<any> }): Promise<Metadata> {
    const params = await props.params;

    const {
        name,
        variant
    } = params;

    const intl = await getIntl();
    const defaultTitle = intl.formatMessage({ id: 'templates' });
    const title = name ? `${defaultTitle} - ${name}:${variant}` : defaultTitle;

    return {
        title
    }
}

export default async function VariantPage(props: PageProps) {
    const params = await props.params;

    const {
        name,
        variant
    } = params;

    const { data: template, errors } = await getVariant(name, variant)
    const intl = await getIntl();
    if (errors) {
        return <ErrorScreen errors={errors} />
    }

    return (
        <div className="lg:w-[60%] sm:[95%] md:[75%] mx-auto">
            <Link
                className={buttonVariants({
                    variant: "link",
                    className: "mx-0! px-0! mb-7 -ml-1! ",
                })}
                href="/templates"
            >
                <ArrowLeftIcon className="w-4 h-4 mr-1.5" /> {intl.formatMessage({id: 'templates.actions.backToAll'})}
            </Link>
            <div className="flex flex-col gap-3 pb-4 w-full mb-2">
                <h1 className="sm:text-4xl text-3xl font-bold">
                    {template.title}
                </h1>
            </div>
            <div className="w-full!">
                <div className="mb-7">
                    <Typography className="mt-2">
                        {template.description}
                    </Typography>
                    <div className="mt-4">
                        <h2 className="sm:text-2xl text-xl font-bold">
                            {intl.formatMessage({id: 'templates.sections.usage'})}
                        </h2>
                        <CodeBlock className="mt-2" language="bash" code={`shoc init ${name}:${variant}`} />
                    </div>
                    <Typography className="mt-4">
                        <h2 className="sm:text-2xl text-xl font-bold">
                            {intl.formatMessage({id: 'templates.sections.overview'})}
                        </h2>
                        <Markdown>
                            {template.overviewMarkdown}
                        </Markdown>
                    </Typography>
                    <Typography className="mt-4">
                        <h2 className="sm:text-2xl text-xl font-bold">
                            {intl.formatMessage({id: 'templates.sections.specification'})}
                        </h2>
                        <Markdown>
                            {template.specificationMarkdown}
                        </Markdown>
                    </Typography>
                    <div className="mt-4">
                        <h2 className="sm:text-2xl text-xl font-bold">
                            {intl.formatMessage({id: 'templates.sections.templateCode'})}
                        </h2>
                        <Accordion className="mt-2" type="single" collapsible>
                            <AccordionItem value="template">
                                <AccordionTrigger>{intl.formatMessage({id: 'templates.sections.template'})}</AccordionTrigger>
                                <AccordionContent>
                                    <CodeBlock language="dockerfile" code={template.containerfile} />
                                </AccordionContent>
                            </AccordionItem>
                            <AccordionItem value="spec">
                                <AccordionTrigger>{intl.formatMessage({id: 'templates.sections.buildSpec'})}</AccordionTrigger>
                                <AccordionContent>
                                    <CodeBlock language="json" code={template.buildSpec} />
                                </AccordionContent>
                            </AccordionItem>
                        </Accordion>
                    </div>
                </div>
            </div>
        </div>
    );
}