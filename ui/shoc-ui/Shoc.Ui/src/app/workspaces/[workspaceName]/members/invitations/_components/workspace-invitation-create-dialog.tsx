"use client"

import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { zodResolver } from "@hookform/resolvers/zod"
import { useIntl } from "react-intl"
import { z } from "zod"
import ErrorAlert from "@/components/general/error-alert"
import { useEffect, useState } from "react"
import { FieldValues, SubmitHandler, useForm } from "react-hook-form"
import { Form, FormControl, FormField, FormItem, FormLabel } from "@/components/ui/form"
import SpinnerIcon from "@/components/icons/spinner-icon"
import { rpc } from "@/server-actions/rpc"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { ModalDialogProps } from "@/components/general/component-types"
import { workspaceRoles, workspaceRolesMap } from "@/well-known/workspace"
import { Input } from "@/components/ui/input"

interface DialogProps extends ModalDialogProps {
  workspaceId: string
}

const ALLOWED_ROLES = ['member', 'guest'];
const workspaceRolesAllowed = workspaceRoles.filter(item => ALLOWED_ROLES.includes(item.key));

export default function WorkspaceInvitationCreateDialog({ workspaceId, open, trigger, onClose, onSuccess }: DialogProps) {

  const intl = useIntl();
  const [errors, setErrors] = useState<any[]>([]);
  const [progress, setProgress] = useState(false);

  const formSchema = z.object({
    email: z.string().email(intl.formatMessage({id: 'global.validation.invalidEmail'})),
    role: z.custom(role => workspaceRolesMap[role], intl.formatMessage({ id: 'workspaces.invitations.validation.invalidRole' })),
  });

  const form = useForm({
    resolver: zodResolver(formSchema),
    shouldUseNativeValidation: false,
    defaultValues: {
      email: '',
      role: ''
    }
  })

  const onOk: SubmitHandler<FieldValues> = async (values) => {

    setErrors([]);
    setProgress(true);

    const { data, errors } = await rpc('workspace/user-workspace-invitations/create', {
      workspaceId: workspaceId,
      input: {
        email: values.email,
        role: values.role
      }
    });

    setProgress(false);

    if (errors) {
      setErrors(errors);
      return;
    }

    if (onSuccess) {
      onSuccess(data)
    }

    onOpenChangeWrapper(false)
  }

  const onOpenChangeWrapper = (openValue: boolean): void => {
    setErrors([]);

    if (!openValue && onClose) {
      onClose();
    }
  }

  useEffect(() => {

    if(!open){
      return;
    }

    form.reset()
    
  }, [form, open]);

  return <Dialog open={open} onOpenChange={onOpenChangeWrapper} modal>
    <DialogTrigger asChild>
      {trigger}
    </DialogTrigger>
    <DialogContent className="w-4/5 md:w-1/2">
      <DialogHeader>
        <DialogTitle>{intl.formatMessage({ id: 'workspaces.invitations.create' })}</DialogTitle>
        <DialogDescription>
          {intl.formatMessage({ id: 'workspaces.invitations.createNotice' })}
        </DialogDescription>
      </DialogHeader>
      <ErrorAlert errors={errors} title={intl.formatMessage({ id: 'workspaces.invitations.createError' })} />
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onOk)}>
          <div className="grid gap-2">
          <div className="grid grap-1">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'global.labels.email' })}</FormLabel>
                    <Input {...field} placeholder={intl.formatMessage({id: 'global.placeholders.email'})} />
                  </FormItem>
                )}
              />
            </div>
            <div className="grid grap-1">
              <FormField
                control={form.control}
                name="role"
                render={({ field: { ref, ...fieldNoRef } }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'workspaces.labels.role' })}</FormLabel>
                    <Select onValueChange={fieldNoRef.onChange} {...fieldNoRef}>
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder={intl.formatMessage({ id: 'workspaces.invitations.placeholders.role' })} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {workspaceRolesAllowed.map((item) => <SelectItem key={item.key} value={item.key}>{intl.formatMessage({ id: item.display })}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </FormItem>
                )}
              />
            </div>
            <DialogFooter>
              <Button type="submit" disabled={progress}>
                {progress && (
                  <SpinnerIcon className="mr-2 h-4 w-4 animate-spin" />
                )}
                {intl.formatMessage({ id: 'workspaces.invitations.create.action' })}
              </Button>
            </DialogFooter>
          </div>
        </form>
      </Form>

    </DialogContent>
  </Dialog>
}